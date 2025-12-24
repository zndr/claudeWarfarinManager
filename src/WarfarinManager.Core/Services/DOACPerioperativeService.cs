using System.Text;
using WarfarinManager.Data.Entities;
using WarfarinManager.Shared.Enums;
using WarfarinManager.Shared.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per la gestione perioperatoria dei DOAC
/// Implementa timing di sospensione/ripresa basato su:
/// - Studio PAUSE (JAMA Intern Med 2019)
/// - Linee guida ASRA 2025
/// - FCSA Position Paper 2021
/// - Nota AIFA 97 Allegato 3
/// </summary>
public class DOACPerioperativeService : IDOACPerioperativeService
{
    private readonly IDOACClinicalService _clinicalService;

    public DOACPerioperativeService(IDOACClinicalService clinicalService)
    {
        _clinicalService = clinicalService;
    }

    public DOACPerioperativeProtocol GenerateProtocol(
        DOACType doacType,
        double egfr,
        BleedingRisk bleedingRisk,
        ThromboembolicRisk teRisk,
        DateTime surgeryDate,
        SurgeryType surgeryType,
        int patientAge,
        double? patientWeight = null)
    {
        var renalFunction = _clinicalService.DetermineRenalFunction(egfr);

        var protocol = new DOACPerioperativeProtocol
        {
            DOACType = doacType,
            RenalFunction = renalFunction,
            EGFR = egfr,
            BleedingRisk = bleedingRisk,
            ThromboembolicRisk = teRisk,
            SurgeryDate = surgeryDate,
            SurgeryType = surgeryType
        };

        // Calcola timing pre-operatorio
        protocol.PreOpSuspensionHours = CalculatePreOpSuspensionHours(doacType, renalFunction, bleedingRisk);
        protocol.LastDOACDose = surgeryDate.AddHours(-protocol.PreOpSuspensionHours);
        protocol.PreOpTimingDescription = FormatPreOpTiming(protocol.PreOpSuspensionHours);

        // Calcola timing post-operatorio
        var (minHours, maxHours) = CalculatePostOpResumeHours(bleedingRisk, teRisk);
        protocol.PostOpMinHours = minHours;
        protocol.PostOpMaxHours = maxHours;
        protocol.DOACResumeDate = surgeryDate.AddHours(minHours);
        protocol.PostOpTimingDescription = FormatPostOpTiming(bleedingRisk, teRisk);

        // Verifica necessità bridging (generalmente NO per DOAC)
        DetermineBridgingStrategy(protocol, teRisk, bleedingRisk);

        // Test livelli plasmatici
        protocol.RecommendPlasmaLevelTesting = ShouldTestPlasmaLevels(patientAge, renalFunction, patientWeight, bleedingRisk);
        protocol.TargetPlasmaLevel = bleedingRisk == BleedingRisk.High ? 30 : 50;

        // Genera raccomandazioni e note
        GenerateRecommendations(protocol, doacType, renalFunction, patientAge, patientWeight);

        return protocol;
    }

    public int CalculatePreOpSuspensionHours(
        DOACType doacType,
        RenalFunction renalFunction,
        BleedingRisk bleedingRisk)
    {
        // Tabelle specifiche per DOAC dalla documentazione

        return doacType switch
        {
            DOACType.Dabigatran => CalculateDabigatranPreOpHours(renalFunction, bleedingRisk),
            DOACType.Rivaroxaban => CalculateRivaroxabanPreOpHours(renalFunction, bleedingRisk),
            DOACType.Apixaban => CalculateApixabanPreOpHours(renalFunction, bleedingRisk),
            DOACType.Edoxaban => CalculateEdoxabanPreOpHours(renalFunction, bleedingRisk),
            _ => 48
        };
    }

    private int CalculateDabigatranPreOpHours(RenalFunction renalFunction, BleedingRisk bleedingRisk)
    {
        // DABIGATRAN - Alta dipendenza renale (80% eliminazione renale)

        if (bleedingRisk == BleedingRisk.High || bleedingRisk == BleedingRisk.Moderate)
        {
            // Alto rischio emorragico
            return renalFunction switch
            {
                RenalFunction.Normal => 48,              // eGFR ≥80: 48h (giorno -3)
                RenalFunction.MildlyReduced => 72,       // eGFR 50-79: 72h (giorno -4)
                RenalFunction.ModeratelyReduced => 96,   // eGFR 30-49: 96h (giorno -5)
                RenalFunction.SeverelyReduced => 96,     // eGFR 15-29: 96h (cautela estrema)
                _ => 96
            };
        }
        else
        {
            // Basso rischio emorragico
            return renalFunction switch
            {
                RenalFunction.Normal => 24,              // eGFR ≥80: 24h (giorno -2)
                RenalFunction.MildlyReduced => 36,       // eGFR 50-79: 36h (giorno -2/3)
                RenalFunction.ModeratelyReduced => 48,   // eGFR 30-49: 48h (giorno -3)
                RenalFunction.SeverelyReduced => 48,     // eGFR 15-29: 48h (cautela)
                _ => 48
            };
        }
    }

    private int CalculateRivaroxabanPreOpHours(RenalFunction renalFunction, BleedingRisk bleedingRisk)
    {
        // RIVAROXABAN - Eliminazione renale intermedia (33%)

        if (bleedingRisk == BleedingRisk.High || bleedingRisk == BleedingRisk.Moderate)
        {
            // Alto/moderato rischio emorragico
            return renalFunction switch
            {
                RenalFunction.SeverelyReduced => 48,     // eGFR 15-29: 48h (cautela)
                _ => 48                                   // Tutti gli altri: 48h (giorno -3)
            };
        }
        else
        {
            // Basso rischio emorragico
            return renalFunction switch
            {
                RenalFunction.SeverelyReduced => 36,     // eGFR 15-29: 36h (cautela)
                _ => 24                                   // Tutti gli altri: 24h (giorno -2)
            };
        }
    }

    private int CalculateApixabanPreOpHours(RenalFunction renalFunction, BleedingRisk bleedingRisk)
    {
        // APIXABAN - Bassa eliminazione renale (27%)

        if (bleedingRisk == BleedingRisk.High || bleedingRisk == BleedingRisk.Moderate)
        {
            // Alto/moderato rischio emorragico
            return renalFunction switch
            {
                RenalFunction.SeverelyReduced => 48,     // eGFR 15-29: 48h (cautela)
                _ => 48                                   // Tutti gli altri: 48h (giorno -3)
            };
        }
        else
        {
            // Basso rischio emorragico
            return renalFunction switch
            {
                RenalFunction.SeverelyReduced => 36,     // eGFR 15-29: 36h (cautela)
                _ => 24                                   // Tutti gli altri: 24h (giorno -2)
            };
        }
    }

    private int CalculateEdoxabanPreOpHours(RenalFunction renalFunction, BleedingRisk bleedingRisk)
    {
        // EDOXABAN - Eliminazione renale intermedia (50%)

        if (bleedingRisk == BleedingRisk.High || bleedingRisk == BleedingRisk.Moderate)
        {
            // Alto/moderato rischio emorragico
            return renalFunction switch
            {
                RenalFunction.SeverelyReduced => 48,     // eGFR 15-29: 48h (cautela)
                _ => 48                                   // Tutti gli altri: 48h (giorno -3)
            };
        }
        else
        {
            // Basso rischio emorragico
            return renalFunction switch
            {
                RenalFunction.SeverelyReduced => 36,     // eGFR 15-29: 36h (cautela)
                _ => 24                                   // Tutti gli altri: 24h (giorno -2)
            };
        }
    }

    public (int minHours, int maxHours) CalculatePostOpResumeHours(
        BleedingRisk bleedingRisk,
        ThromboembolicRisk teRisk)
    {
        // Timing post-operatorio uguale per tutti i DOAC

        return bleedingRisk switch
        {
            BleedingRisk.Low when teRisk == ThromboembolicRisk.Low => (6, 8),         // Minimo: 6-8h
            BleedingRisk.Low => (6, 8),                                               // Basso: 6-8h
            BleedingRisk.Moderate when teRisk == ThromboembolicRisk.High => (24, 24), // Moderato + alto TE: 24h + EBPM se differita
            BleedingRisk.Moderate => (24, 24),                                        // Moderato: 24h
            BleedingRisk.High when teRisk == ThromboembolicRisk.High => (72, 120),   // Alto + alto TE: 72-120h + EBPM
            BleedingRisk.High => (48, 72),                                            // Alto: 48-72h
            _ => (24, 24)
        };
    }

    public bool ShouldTestPlasmaLevels(
        int patientAge,
        RenalFunction renalFunction,
        double? patientWeight,
        BleedingRisk bleedingRisk)
    {
        // Raccomandato test preoperatorio in:

        // 1. Età >75 anni
        if (patientAge > 75) return true;

        // 2. Insufficienza renale severa
        if (renalFunction == RenalFunction.SeverelyReduced) return true;

        // 3. Peso <50 kg
        if (patientWeight.HasValue && patientWeight.Value < 50) return true;

        // 4. Procedure ad altissimo rischio
        if (bleedingRisk == BleedingRisk.High) return true;

        return false;
    }

    private void DetermineBridgingStrategy(
        DOACPerioperativeProtocol protocol,
        ThromboembolicRisk teRisk,
        BleedingRisk bleedingRisk)
    {
        // PRINCIPIO FONDAMENTALE: Il bridging NON è raccomandato per DOAC
        // Emivita breve (8-15 ore) rende superfluo il bridging routinario
        // Studio BRIDGE: bridging aumenta rischio emorragico senza ridurre eventi trombotici

        protocol.NoBridgingRecommended = true;
        protocol.BridgingRequired = false;

        // ECCEZIONE: Solo se ALTO rischio TE + ritardo intervento >48-72h
        if (teRisk == ThromboembolicRisk.High && protocol.PreOpSuspensionHours > 72)
        {
            protocol.BridgingRequired = true;
            protocol.EBPMType = "Enoxaparina";
            protocol.EBPMDosage = "70 UI/kg bid (dose intermedia)";
            protocol.ClinicalNotes.Add("Bridging con EBPM dose intermedia solo se sospensione >72h + alto rischio TE");
            protocol.ClinicalNotes.Add("Iniziare EBPM dopo 24-48h dalla sospensione DOAC");
            protocol.ClinicalNotes.Add("Ultima dose EBPM 24h pre-operatoria");
        }

        if (teRisk == ThromboembolicRisk.High && bleedingRisk == BleedingRisk.High)
        {
            protocol.Warnings.Add("⚠ ALTO RISCHIO TE + ALTO RISCHIO EMORRAGICO: valutazione multidisciplinare giornaliera");
            protocol.ClinicalNotes.Add("Monitorare: Hb, piastrine, creatinina, drenaggi, temperatura, guarigione ferita");
        }
    }

    private void GenerateRecommendations(
        DOACPerioperativeProtocol protocol,
        DOACType doacType,
        RenalFunction renalFunction,
        int patientAge,
        double? patientWeight)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"GESTIONE PERIOPERATORIA {doacType.ToString().ToUpper()}");
        sb.AppendLine();
        sb.AppendLine("Raccomandazione principale:");
        sb.AppendLine($"• Sospensione: {protocol.PreOpTimingDescription} prima dell'intervento");
        sb.AppendLine($"• Ripresa: {protocol.PostOpTimingDescription} post-operatorio (se emostasi adeguata)");

        if (protocol.NoBridgingRecommended && !protocol.BridgingRequired)
        {
            sb.AppendLine("• Bridging: NON raccomandato (emivita breve DOAC)");
        }

        protocol.MainRecommendation = sb.ToString();

        // Avvertenze specifiche per DOAC

        if (doacType == DOACType.Dabigatran && renalFunction >= RenalFunction.ModeratelyReduced)
        {
            protocol.Warnings.Add("⚠ DABIGATRAN + INSUFFICIENZA RENALE: maggiore accumulo (eliminazione renale 80%)");
            protocol.Warnings.Add("Considerare sospensione più precoce con eGFR <50 mL/min");
        }

        if (protocol.RecommendPlasmaLevelTesting)
        {
            protocol.Warnings.Add($"⚠ RACCOMANDATO dosaggio livelli plasmatici DOAC pre-operatori (target <{protocol.TargetPlasmaLevel} ng/mL)");
            protocol.ClinicalNotes.Add("Test 12-24h pre-operatori in: età >75, IR severa, peso <50 kg, alto rischio emorragico");
        }

        // Note cliniche generali

        protocol.ClinicalNotes.Add("Emivita DOAC: 8-15 ore (molto più breve di warfarin)");
        protocol.ClinicalNotes.Add("Bridging routinario NON necessario (a differenza di warfarin)");
        protocol.ClinicalNotes.Add("Valutare emostasi locale prima di riprendere DOAC");

        if (protocol.BridgingRequired)
        {
            protocol.ClinicalNotes.Add("Sospendere EBPM contestualmente a ripresa DOAC dose piena");
        }

        // Note specifiche per tipo chirurgia

        AddSurgerySpecificNotes(protocol);

        // Evidenze cliniche

        protocol.ClinicalNotes.Add("");
        protocol.ClinicalNotes.Add("EVIDENZE: Studio PAUSE (JAMA Intern Med 2019):");
        protocol.ClinicalNotes.Add("• 3.007 pazienti FA con strategia standardizzata senza bridging");
        protocol.ClinicalNotes.Add("• Sanguinamento maggiore: 0.9-1.85% (sicuro)");
        protocol.ClinicalNotes.Add("• Tromboembolismo: 0.16-0.6% (molto basso)");
    }

    private void AddSurgerySpecificNotes(DOACPerioperativeProtocol protocol)
    {
        switch (protocol.SurgeryType)
        {
            case SurgeryType.Neurosurgery:
            case SurgeryType.EpiduralAnesthesia:
                protocol.Warnings.Add("🔴 NEUROCHIRURGIA/ANESTESIA NEUROASSIALE:");
                protocol.Warnings.Add("Timing ASRA 2025: sospensione ≥72h, ripresa ≥24h dopo rimozione ago/catetere");
                protocol.Warnings.Add("Rischio ematoma epidurale con paralisi permanente");
                protocol.ClinicalNotes.Add("Rimozione catetere epidurale: ≥5h prima ripresa DOAC");
                protocol.ClinicalNotes.Add("Valutare dosaggio livelli plasmatici (target <30 ng/mL)");
                break;

            case SurgeryType.DentalSingleExtraction:
            case SurgeryType.DentalMajor:
                protocol.ClinicalNotes.Add("Procedure dentali: considerare continuazione DOAC con emostasi locale");
                protocol.ClinicalNotes.Add("Emostasi: acido tranexamico collutorio, garze emostatiche");
                break;

            case SurgeryType.DiagnosticEndoscopy:
            case SurgeryType.DiagnosticColonoscopy:
                protocol.ClinicalNotes.Add("Endoscopia diagnostica: rischio minimo, saltare 1 dose (18-24h) può essere sufficiente");
                break;

            case SurgeryType.EndoscopyWithBiopsy:
            case SurgeryType.Polypectomy:
                protocol.ClinicalNotes.Add("Polipectomia: rischio sanguinamento ritardato a 7-14 giorni (fase guarigione mucosa)");
                protocol.ClinicalNotes.Add("Valutare ripresa differita DOAC fino a 72h post-procedura");
                break;

            case SurgeryType.CardiacSurgery:
            case SurgeryType.VascularSurgery:
                protocol.ClinicalNotes.Add("Coordinare gestione anticoagulazione con team cardiochirurgico/vascolare");
                break;

            case SurgeryType.PacemakerImplant:
                protocol.ClinicalNotes.Add("Impianto dispositivi cardiaci: considerare continuazione DOAC in pazienti selezionati");
                protocol.ClinicalNotes.Add("Schedulare procedura a valle picco (12-24h da ultima dose)");
                break;
        }
    }

    private string FormatPreOpTiming(int hours)
    {
        int days = hours / 24;
        return hours switch
        {
            24 => "24h (giorno -2)",
            36 => "36h (giorno -2/3)",
            48 => "48h (giorno -3)",
            72 => "72h (giorno -4)",
            96 => "96h (giorno -5)",
            120 => "120h (giorno -6)",
            _ => $"{hours}h ({days} giorni)"
        };
    }

    private string FormatPostOpTiming(BleedingRisk bleedingRisk, ThromboembolicRisk teRisk)
    {
        return bleedingRisk switch
        {
            BleedingRisk.Low => "6-8h (stessa giornata)",
            BleedingRisk.Moderate when teRisk == ThromboembolicRisk.High => "24h + EBPM profilattica se differita",
            BleedingRisk.Moderate => "24h (giorno +1)",
            BleedingRisk.High when teRisk == ThromboembolicRisk.High => "72-120h (giorno +3/5) + EBPM intermedia",
            BleedingRisk.High => "48-72h (giorno +2/3)",
            _ => "24h (giorno +1)"
        };
    }

    public string FormatProtocolForExport(DOACPerioperativeProtocol protocol, Patient patient)
    {
        var sb = new StringBuilder();

        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("  TaoGEST - PROTOCOLLO GESTIONE PERIOPERATORIA DOAC");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine();

        sb.AppendLine($"PAZIENTE: {patient.LastName} {patient.FirstName}");
        sb.AppendLine($"CF: {patient.FiscalCode}");
        sb.AppendLine($"Data di nascita: {patient.BirthDate:dd/MM/yyyy} ({patient.Age} anni)");
        sb.AppendLine($"Anticoagulante: {protocol.DOACType}");
        sb.AppendLine();

        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine("INFORMAZIONI INTERVENTO");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"Data intervento: {protocol.SurgeryDate:dddd dd MMMM yyyy HH:mm}");
        sb.AppendLine($"Tipo chirurgia: {GetSurgeryTypeDescription(protocol.SurgeryType)}");
        sb.AppendLine($"Rischio emorragico: {GetRiskDescription(protocol.BleedingRisk)}");
        sb.AppendLine();

        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine("VALUTAZIONE PAZIENTE");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"Funzione renale: {GetRenalFunctionDescription(protocol.RenalFunction)}");
        sb.AppendLine($"eGFR: {protocol.EGFR:F1} mL/min");
        sb.AppendLine($"Rischio tromboembolico: {GetRiskDescription(protocol.ThromboembolicRisk)}");
        sb.AppendLine();

        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("PROTOCOLLO RACCOMANDATO");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine(protocol.MainRecommendation);
        sb.AppendLine();

        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine("TIMELINE PRE-OPERATORIA");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"  Ultima dose {protocol.DOACType}: {protocol.LastDOACDose:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"  Sospensione: {protocol.PreOpTimingDescription}");

        if (protocol.RecommendPlasmaLevelTesting)
        {
            var testDate = protocol.SurgeryDate.AddHours(-12);
            sb.AppendLine($"  Test livelli plasmatici: {testDate:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"      → Target: <{protocol.TargetPlasmaLevel} ng/mL");
        }

        sb.AppendLine($"  Giorno intervento: {protocol.SurgeryDate:dd/MM/yyyy HH:mm} *** CHIRURGIA ***");
        sb.AppendLine();

        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine("TIMELINE POST-OPERATORIA");
        sb.AppendLine("───────────────────────────────────────────────────────────────");
        sb.AppendLine();
        sb.AppendLine($"  Ripresa {protocol.DOACType}: {protocol.DOACResumeDate:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"  Timing: {protocol.PostOpTimingDescription}");
        sb.AppendLine("      → Solo se emostasi adeguata");

        if (protocol.BridgingRequired)
        {
            sb.AppendLine();
            sb.AppendLine("  ⚠ Bridging necessario (alto rischio TE + lunga sospensione):");
            sb.AppendLine($"      → {protocol.EBPMType} {protocol.EBPMDosage}");
            sb.AppendLine("      → Stop EBPM quando DOAC ripreso a dose piena");
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("  ✓ Bridging: NON raccomandato (emivita breve DOAC)");
        }
        sb.AppendLine();

        if (protocol.Warnings.Any())
        {
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine("⚠ AVVERTENZE CLINICHE");
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine();
            foreach (var warning in protocol.Warnings)
            {
                sb.AppendLine($"  {warning}");
            }
            sb.AppendLine();
        }

        if (protocol.ClinicalNotes.Any())
        {
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine("NOTE CLINICHE");
            sb.AppendLine("───────────────────────────────────────────────────────────────");
            sb.AppendLine();
            foreach (var note in protocol.ClinicalNotes)
            {
                sb.AppendLine($"  • {note}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine($"Generato da: TaoGEST - Gestione Terapia Anticoagulante Orale");
        sb.AppendLine($"Data/Ora: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("DISCLAIMER: Questo protocollo è uno strumento di supporto");
        sb.AppendLine("decisionale basato su linee guida FCSA 2021, Studio PAUSE,");
        sb.AppendLine("ASRA 2025. Il medico prescrittore è responsabile della");
        sb.AppendLine("valutazione clinica finale e della decisione terapeutica.");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");

        return sb.ToString();
    }

    #region Helper Methods

    private string GetSurgeryTypeDescription(SurgeryType type)
    {
        return type switch
        {
            SurgeryType.DiagnosticEndoscopy => "Endoscopia diagnostica",
            SurgeryType.DiagnosticColonoscopy => "Colonscopia diagnostica",
            SurgeryType.CardiacCatheterization => "Cateterismo cardiaco diagnostico",
            SurgeryType.Transesophageal => "Ecocardiografia transesofagea",
            SurgeryType.DermatologySingleExcision => "Escissione cutanea minore",
            SurgeryType.Ophthalmology => "Chirurgia oculistica minore",
            SurgeryType.DentalSingleExtraction => "Estrazione dentaria singola",
            SurgeryType.EndoscopyWithBiopsy => "Endoscopia con biopsia",
            SurgeryType.Polypectomy => "Polipectomia colonscopica",
            SurgeryType.Cardioversion => "Cardioversione elettrica",
            SurgeryType.PacemakerImplant => "Impianto pacemaker/ICD",
            SurgeryType.ArthroscopyMinor => "Artroscopia minore",
            SurgeryType.LaparoscopicCholecystectomy => "Colecistectomia laparoscopica",
            SurgeryType.TURP => "TURP / Cistoscopia con biopsia",
            SurgeryType.Neurosurgery => "Neurochirurgia",
            SurgeryType.CardiacSurgery => "Cardiochirurgia",
            SurgeryType.VascularSurgery => "Chirurgia vascolare maggiore",
            SurgeryType.ThoracicSurgery => "Chirurgia toracica",
            SurgeryType.AbdominalSurgery => "Chirurgia addominale maggiore",
            SurgeryType.HepaticSurgery => "Chirurgia epatica",
            SurgeryType.PancreaticSurgery => "Chirurgia pancreatica",
            SurgeryType.ProstateSurgery => "Chirurgia prostatica",
            SurgeryType.RenalSurgery => "Chirurgia renale",
            SurgeryType.MajorOrthopedic => "Chirurgia ortopedica maggiore",
            SurgeryType.EpiduralAnesthesia => "Anestesia neuroassiale",
            SurgeryType.OphthalmologySurgery => "Chirurgia oculistica maggiore",
            SurgeryType.DentalMajor => "Chirurgia odontoiatrica maggiore",
            SurgeryType.Other => "Altra procedura",
            _ => type.ToString()
        };
    }

    private string GetRiskDescription(BleedingRisk risk)
    {
        return risk switch
        {
            BleedingRisk.Low => "BASSO (0-2%)",
            BleedingRisk.Moderate => "MODERATO (2-5%)",
            BleedingRisk.High => "ALTO (>5%)",
            _ => risk.ToString()
        };
    }

    private string GetRiskDescription(ThromboembolicRisk risk)
    {
        return risk switch
        {
            ThromboembolicRisk.Low => "BASSO",
            ThromboembolicRisk.Moderate => "MODERATO",
            ThromboembolicRisk.High => "ALTO (>10%/anno)",
            _ => risk.ToString()
        };
    }

    private string GetRenalFunctionDescription(RenalFunction function)
    {
        return function switch
        {
            RenalFunction.Normal => "Normale (eGFR ≥80)",
            RenalFunction.MildlyReduced => "Lievemente ridotta (eGFR 50-79)",
            RenalFunction.ModeratelyReduced => "Moderatamente ridotta (eGFR 30-49)",
            RenalFunction.SeverelyReduced => "Severamente ridotta (eGFR 15-29)",
            RenalFunction.EndStage => "Insufficienza renale terminale (eGFR <15 o dialisi)",
            _ => function.ToString()
        };
    }

    #endregion
}
