using WarfarinManager.Shared.Enums;
using WarfarinManager.Shared.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per la gestione di indicazioni e controindicazioni DOAC
/// Basato su RCP EMA e linee guida EHRA 2018
/// </summary>
public class DOACClinicalService : IDOACClinicalService
{
    private readonly List<DOACIndication> _indications;
    private readonly List<DOACContraindication> _contraindications;

    public DOACClinicalService()
    {
        _indications = InitializeIndications();
        _contraindications = InitializeContraindications();
    }

    public List<DOACIndication> GetApprovedIndications(DOACType doacType)
    {
        return _indications.Where(i => i.DOACType == doacType && i.IsEMAApproved).ToList();
    }

    public bool IsIndicationApproved(DOACType doacType, string indicationCode)
    {
        return _indications.Any(i =>
            i.DOACType == doacType &&
            i.Code.Equals(indicationCode, StringComparison.OrdinalIgnoreCase) &&
            i.IsEMAApproved);
    }

    public List<DOACContraindication> GetContraindications(DOACType doacType)
    {
        return _contraindications.Where(c => c.DOACType == doacType).ToList();
    }

    public List<DOACContraindication> GetAbsoluteContraindications(DOACType doacType)
    {
        return _contraindications.Where(c => c.DOACType == doacType && c.IsAbsolute).ToList();
    }

    public List<DOACContraindication> GetPrecautions(DOACType doacType)
    {
        return _contraindications.Where(c => c.DOACType == doacType && !c.IsAbsolute).ToList();
    }

    public RenalFunction DetermineRenalFunction(double egfr)
    {
        return egfr switch
        {
            >= 80 => RenalFunction.Normal,
            >= 50 => RenalFunction.MildlyReduced,
            >= 30 => RenalFunction.ModeratelyReduced,
            >= 15 => RenalFunction.SeverelyReduced,
            _ => RenalFunction.EndStage
        };
    }

    public bool IsContraindicatedWithRenalFunction(DOACType doacType, RenalFunction renalFunction)
    {
        return doacType switch
        {
            DOACType.Dabigatran => renalFunction == RenalFunction.EndStage,
            DOACType.Rivaroxaban => renalFunction == RenalFunction.EndStage,
            DOACType.Apixaban => renalFunction == RenalFunction.EndStage,
            DOACType.Edoxaban => renalFunction == RenalFunction.EndStage,
            _ => false
        };
    }

    public bool RequiresDoseReductionForRenalFunction(DOACType doacType, RenalFunction renalFunction)
    {
        return doacType switch
        {
            DOACType.Dabigatran => renalFunction >= RenalFunction.ModeratelyReduced,
            DOACType.Rivaroxaban => renalFunction >= RenalFunction.SeverelyReduced,
            DOACType.Apixaban => renalFunction >= RenalFunction.SeverelyReduced,
            DOACType.Edoxaban => renalFunction >= RenalFunction.ModeratelyReduced,
            _ => false
        };
    }

    private List<DOACIndication> InitializeIndications()
    {
        var indications = new List<DOACIndication>();

        // ========== DABIGATRAN ==========
        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Dabigatran,
            Code = "TEV_PREVENTION_ORTHOPEDIC",
            Description = "Prevenzione primaria TEV in chirurgia sostitutiva elettiva totale anca/ginocchio",
            Population = "Adulta"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Dabigatran,
            Code = "FANV_STROKE_PREVENTION",
            Description = "Prevenzione ictus ed embolia sistemica in FANV con ≥1 fattori di rischio",
            Population = "Adulta"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Dabigatran,
            Code = "TEV_TREATMENT",
            Description = "Trattamento TVP ed EP e prevenzione recidive",
            Population = "Adulta"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Dabigatran,
            Code = "TEV_PEDIATRIC",
            Description = "Trattamento TEV e prevenzione recidive (dopo ≥5 giorni eparina)",
            Population = "Pediatrica (capace ingerire cibo morbido fino <18 anni)",
            Notes = "Dopo almeno 5 giorni di terapia anticoagulante parenterale iniziale"
        });

        // ========== RIVAROXABAN ==========
        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Rivaroxaban,
            Code = "ACS_PREVENTION",
            Description = "Prevenzione eventi aterotrombotici dopo SCA con biomarcatori elevati (+ ASA ± clopidogrel/ticlopidina)",
            Population = "Adulta",
            Notes = "Formulazione 2.5 mg"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Rivaroxaban,
            Code = "CAD_PAD_PREVENTION",
            Description = "Prevenzione eventi aterotrombotici in CAD o PAD sintomatica ad alto rischio (+ ASA)",
            Population = "Adulta",
            Notes = "Formulazione 2.5 mg"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Rivaroxaban,
            Code = "TEV_PREVENTION_ORTHOPEDIC",
            Description = "Prevenzione TEV in chirurgia elettiva sostitutiva anca/ginocchio",
            Population = "Adulta",
            Notes = "Formulazioni 10/15/20 mg"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Rivaroxaban,
            Code = "FANV_STROKE_PREVENTION",
            Description = "Prevenzione ictus ed embolia sistemica in FANV con ≥1 fattori di rischio",
            Population = "Adulta",
            Notes = "Formulazioni 10/15/20 mg"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Rivaroxaban,
            Code = "TEV_TREATMENT",
            Description = "Trattamento TVP ed EP e prevenzione recidive",
            Population = "Adulta",
            Notes = "Formulazioni 10/15/20 mg"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Rivaroxaban,
            Code = "TEV_PEDIATRIC",
            Description = "Trattamento TEV e prevenzione recidive (dopo ≥5 giorni eparina)",
            Population = "Pediatrica (neonati a termine e bambini <18 anni)"
        });

        // ========== APIXABAN ==========
        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Apixaban,
            Code = "TEV_PREVENTION_ORTHOPEDIC",
            Description = "Prevenzione TEV in chirurgia sostitutiva elettiva anca/ginocchio",
            Population = "Adulta"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Apixaban,
            Code = "FANV_STROKE_PREVENTION",
            Description = "Prevenzione ictus ed embolia sistemica in FANV con ≥1 fattori di rischio",
            Population = "Adulta",
            Notes = "Fattori: ictus/TIA pregresso, età ≥75, ipertensione, diabete, scompenso NYHA ≥II"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Apixaban,
            Code = "TEV_TREATMENT",
            Description = "Trattamento TVP ed EP e prevenzione recidive",
            Population = "Adulta"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Apixaban,
            Code = "TEV_PEDIATRIC",
            Description = "Trattamento TEV e prevenzione recidive (dopo ≥5 giorni eparina)",
            Population = "Pediatrica (da 28 giorni a <18 anni)"
        });

        // ========== EDOXABAN ==========
        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Edoxaban,
            Code = "FANV_STROKE_PREVENTION",
            Description = "Prevenzione ictus ed embolia sistemica in FANV con ≥1 fattori di rischio",
            Population = "Adulta",
            Notes = "Fattori: scompenso, ipertensione, età ≥75, diabete, ictus/TIA pregresso"
        });

        indications.Add(new DOACIndication
        {
            DOACType = DOACType.Edoxaban,
            Code = "TEV_TREATMENT",
            Description = "Trattamento TVP ed EP e prevenzione recidive (dopo ≥5 giorni eparina)",
            Population = "Adulta"
        });

        return indications;
    }

    private List<DOACContraindication> InitializeContraindications()
    {
        var contraindications = new List<DOACContraindication>();

        // ========== DABIGATRAN - Specifiche ==========
        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Dabigatran,
            Level = "Assoluta",
            Description = "Compromissione renale severa (CLCr <30 mL/min) in adulti",
            Details = "Eliminazione renale 80% - maggiore dipendenza dalla funzione renale"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Dabigatran,
            Level = "Assoluta",
            Description = "Uso concomitante con inibitori forti P-gp: ketoconazolo sistemico, ciclosporina, itraconazolo, dronedarone, glecaprevir/pibrentasvir"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Dabigatran,
            Level = "Assoluta",
            Description = "Protesi valvolari cardiache che richiedano trattamento anticoagulante"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Dabigatran,
            Level = "Precauzione",
            Description = "Insufficienza renale moderata (CLCr 30-50 mL/min)",
            Details = "Riduzione dose raccomandata (150 mg/die vs 220 mg/die)"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Dabigatran,
            Level = "Precauzione",
            Description = "Età ≥75 anni",
            Details = "Riduzione dose raccomandata"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Dabigatran,
            Level = "Precauzione",
            Description = "Peso <50 kg o >110 kg",
            Details = "Stretto controllo clinico (esperienza limitata)"
        });

        // ========== RIVAROXABAN - Specifiche ==========
        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Rivaroxaban,
            Level = "Assoluta",
            Description = "Trattamento SCA con antipiastrinici in pazienti con pregresso ictus o TIA"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Rivaroxaban,
            Level = "Assoluta",
            Description = "Trattamento CAD/PAD con ASA in pazienti con ictus emorragico/lacunare o ictus nel mese precedente"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Rivaroxaban,
            Level = "Assoluta",
            Description = "Patologie epatiche con coagulopatia (Child-Pugh B-C)"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Rivaroxaban,
            Level = "Precauzione",
            Description = "Compromissione renale severa (CLCr 15-29 mL/min)",
            Details = "Usare con cautela; CLCr <15 mL/min: uso NON raccomandato"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Rivaroxaban,
            Level = "Precauzione",
            Description = "Età ≥75 anni",
            Details = "Cautela (↑ rischio emorragico)"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Rivaroxaban,
            Level = "Precauzione",
            Description = "Peso <60 kg",
            Details = "Cautela e monitoraggio"
        });

        // ========== APIXABAN - Specifiche ==========
        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Apixaban,
            Level = "Assoluta",
            Description = "Malattia epatica associata a coagulopatia e rischio sanguinamento clinicamente rilevante"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Apixaban,
            Level = "Precauzione",
            Description = "Compromissione renale severa (CLCr 15-29 mL/min)",
            Details = "Usare con cautela; riduzione dose 2.5 mg bid se ≥2 criteri FANV (età ≥80, peso ≤60 kg, creatinina ≥1.5 mg/dL)"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Apixaban,
            Level = "Assoluta",
            Description = "CLCr <15 mL/min o dialisi",
            Details = "NON raccomandato"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Apixaban,
            Level = "Precauzione",
            Description = "Compromissione epatica severa",
            Details = "NON raccomandato; Child-Pugh A-B: cautela"
        });

        // ========== EDOXABAN - Specifiche ==========
        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Edoxaban,
            Level = "Assoluta",
            Description = "Ipertensione severa non controllata"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Edoxaban,
            Level = "Assoluta",
            Description = "Malattia epatica associata a coagulopatia e rischio emorragico clinicamente rilevante"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Edoxaban,
            Level = "Precauzione",
            Description = "Compromissione renale moderata-severa (CLCr 15-50 mL/min)",
            Details = "Riduzione dose (30 mg/die vs 60 mg/die)"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Edoxaban,
            Level = "Assoluta",
            Description = "CLCr <15 mL/min o dialisi",
            Details = "Uso NON raccomandato"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Edoxaban,
            Level = "Precauzione",
            Description = "Peso ≤60 kg",
            Details = "Riduzione dose (30 mg/die)"
        });

        contraindications.Add(new DOACContraindication
        {
            DOACType = DOACType.Edoxaban,
            Level = "Precauzione",
            Description = "CrCl elevata nella FANV",
            Details = "Tendenza a riduzione efficacia vs warfarin; usare solo dopo attenta valutazione"
        });

        // ========== CONTROINDICAZIONI COMUNI A TUTTI I DOAC ==========
        foreach (var doacType in Enum.GetValues<DOACType>())
        {
            contraindications.Add(new DOACContraindication
            {
                DOACType = doacType,
                Level = "Assoluta",
                Description = "Sanguinamento attivo clinicamente significativo"
            });

            contraindications.Add(new DOACContraindication
            {
                DOACType = doacType,
                Level = "Assoluta",
                Description = "Lesioni ad alto rischio sanguinamento maggiore",
                Details = "Ulcera GI recente, neoplasie ad alto rischio, trauma cerebrale/spinale, chirurgia neurochirurgica/oculistica recente, emorragia intracranica recente, varici esofagee, malformazioni AV"
            });

            contraindications.Add(new DOACContraindication
            {
                DOACType = doacType,
                Level = "Assoluta",
                Description = "Trattamento concomitante con altri anticoagulanti",
                Details = "Salvo specifiche circostanze di switching terapeutico"
            });

            contraindications.Add(new DOACContraindication
            {
                DOACType = doacType,
                Level = "Assoluta",
                Description = "Gravidanza e allattamento"
            });

            contraindications.Add(new DOACContraindication
            {
                DOACType = doacType,
                Level = "Precauzione",
                Description = "Sindrome antifosfolipidica",
                Details = "DOAC NON raccomandati (particolare cautela nei pazienti triplo-positivi)"
            });

            contraindications.Add(new DOACContraindication
            {
                DOACType = doacType,
                Level = "Precauzione",
                Description = "Protesi valvolari meccaniche e stenosi mitralica moderata-severa",
                Details = "DOAC non raccomandati (sicurezza ed efficacia non stabilite)"
            });

            contraindications.Add(new DOACContraindication
            {
                DOACType = doacType,
                Level = "Precauzione",
                Description = "Cancro attivo",
                Details = "Valutazione individualizzata rischio/beneficio (↑ rischio emorragico, specialmente tumori GI/GU)"
            });
        }

        return contraindications;
    }
}
