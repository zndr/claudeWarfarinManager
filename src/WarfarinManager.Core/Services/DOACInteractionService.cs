using WarfarinManager.Shared.Enums;
using WarfarinManager.Shared.Models;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Servizio per la gestione delle interazioni farmacologiche dei DOAC
/// Basato sui dati EMA RCP e linee guida EHRA 2018
/// </summary>
public class DOACInteractionService : IDOACInteractionService
{
    private readonly List<DOACInteraction> _interactions;

    public DOACInteractionService()
    {
        _interactions = InitializeInteractions();
    }

    public List<DOACInteraction> GetDangerousInteractions(DOACType doacType)
    {
        return _interactions
            .Where(i => i.DOACType == doacType && i.IsDangerous)
            .OrderByDescending(i => i.InteractionLevel)
            .ToList();
    }

    public List<DOACInteraction> GetAllInteractions(DOACType doacType)
    {
        return _interactions
            .Where(i => i.DOACType == doacType)
            .OrderByDescending(i => i.InteractionLevel)
            .ThenBy(i => i.DrugClass)
            .ToList();
    }

    public DOACInteraction? CheckInteraction(DOACType doacType, string drugName)
    {
        if (string.IsNullOrWhiteSpace(drugName))
            return null;

        var normalizedDrug = NormalizeDrugName(drugName);

        // Prima cerca corrispondenza esatta
        var exactMatch = _interactions.FirstOrDefault(i =>
            i.DOACType == doacType &&
            i.DrugName.Equals(drugName, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
            return exactMatch;

        // Poi cerca con nome normalizzato nel DrugName
        var normalizedMatch = _interactions.FirstOrDefault(i =>
            i.DOACType == doacType &&
            (NormalizeDrugName(i.DrugName).Contains(normalizedDrug) ||
             normalizedDrug.Contains(NormalizeDrugName(i.DrugName))));

        if (normalizedMatch != null)
            return normalizedMatch;

        // Cerca tra i sinonimi noti
        var mappedName = GetMappedDrugName(normalizedDrug);
        if (!string.IsNullOrEmpty(mappedName))
        {
            return _interactions.FirstOrDefault(i =>
                i.DOACType == doacType &&
                NormalizeDrugName(i.DrugName).Contains(mappedName));
        }

        return null;
    }

    /// <summary>
    /// Normalizza il nome del farmaco per confronti
    /// </summary>
    private static string NormalizeDrugName(string drugName)
    {
        return drugName
            .ToLowerInvariant()
            .Replace("-", " ")
            .Replace("_", " ")
            .Replace("(", "")
            .Replace(")", "")
            .Replace(",", "")
            .Replace(".", "")
            .Trim();
    }

    /// <summary>
    /// Mappa sinonimi e nomi commerciali ai nomi standard
    /// </summary>
    private static string? GetMappedDrugName(string normalizedDrug)
    {
        // Dizionario sinonimi -> nome standard nel database
        var synonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Azoli antifungini
            { "ketoconazolo", "ketoconazolo" },
            { "nizoral", "ketoconazolo" },
            { "itraconazolo", "itraconazolo" },
            { "sporanox", "itraconazolo" },
            { "voriconazolo", "voriconazolo" },
            { "vfend", "voriconazolo" },
            { "posaconazolo", "posaconazolo" },
            { "noxafil", "posaconazolo" },
            { "fluconazolo", "fluconazolo" },
            { "diflucan", "fluconazolo" },

            // Inibitori proteasi HIV
            { "ritonavir", "ritonavir" },
            { "norvir", "ritonavir" },
            { "cobicistat", "ritonavir" },
            { "tybost", "ritonavir" },

            // Induttori
            { "rifampicina", "rifampicina" },
            { "rifampin", "rifampicina" },
            { "rifadin", "rifampicina" },
            { "rimactan", "rifampicina" },
            { "carbamazepina", "carbamazepina" },
            { "tegretol", "carbamazepina" },
            { "fenitoina", "fenitoina" },
            { "phenytoin", "fenitoina" },
            { "dintoina", "fenitoina" },
            { "dilantin", "fenitoina" },
            { "fenobarbital", "fenobarbital" },
            { "phenobarbital", "fenobarbital" },
            { "gardenale", "fenobarbital" },
            { "iperico", "erba di san giovanni" },
            { "hypericum", "erba di san giovanni" },
            { "st john", "erba di san giovanni" },

            // Antiaritmici
            { "dronedarone", "dronedarone" },
            { "multaq", "dronedarone" },
            { "amiodarone", "amiodarone" },
            { "cordarone", "amiodarone" },
            { "chinidina", "chinidina" },
            { "quinidina", "chinidina" },
            { "verapamil", "verapamil" },
            { "isoptin", "verapamil" },
            { "diltiazem", "diltiazem" },

            // Immunosoppressori
            { "ciclosporina", "ciclosporina" },
            { "ciclosporin", "ciclosporina" },
            { "cyclosporine", "ciclosporina" },
            { "sandimmun", "ciclosporina" },
            { "neoral", "ciclosporina" },
            { "tacrolimus", "ciclosporina" },

            // Macrolidi
            { "claritromicina", "claritromicina" },
            { "clarithromycin", "claritromicina" },
            { "klacid", "claritromicina" },
            { "eritromicina", "eritromicina" },
            { "erythromycin", "eritromicina" },

            // Antipiastrinici
            { "acido acetilsalicilico", "asa" },
            { "aspirina", "asa" },
            { "aspirin", "asa" },
            { "cardioaspirina", "asa" },
            { "clopidogrel", "clopidogrel" },
            { "plavix", "clopidogrel" },
            { "ticagrelor", "ticagrelor" },
            { "brilique", "ticagrelor" },
            { "prasugrel", "prasugrel" },
            { "efient", "prasugrel" },

            // FANS
            { "ibuprofene", "fans" },
            { "ibuprofen", "fans" },
            { "brufen", "fans" },
            { "moment", "fans" },
            { "naprossene", "fans" },
            { "naproxen", "fans" },
            { "momendol", "fans" },
            { "synflex", "fans" },
            { "diclofenac", "fans" },
            { "voltaren", "fans" },
            { "ketoprofene", "fans" },
            { "ketoprofen", "fans" },
            { "oki", "fans" },
            { "orudis", "fans" },
            { "indometacina", "fans" },
            { "indomethacin", "fans" },
            { "celecoxib", "fans" },
            { "celebrex", "fans" },
            { "etoricoxib", "fans" },
            { "arcoxia", "fans" },
            { "nimesulide", "fans" },
            { "aulin", "fans" },
            { "piroxicam", "fans" },
            { "feldene", "fans" },

            // Antidepressivi SSRI/SNRI
            { "sertralina", "ssri" },
            { "sertraline", "ssri" },
            { "zoloft", "ssri" },
            { "paroxetina", "ssri" },
            { "paroxetine", "ssri" },
            { "sereupin", "ssri" },
            { "fluoxetina", "ssri" },
            { "fluoxetine", "ssri" },
            { "prozac", "ssri" },
            { "citalopram", "ssri" },
            { "elopram", "ssri" },
            { "escitalopram", "ssri" },
            { "cipralex", "ssri" },
            { "entact", "ssri" },
            { "venlafaxina", "snri" },
            { "venlafaxine", "snri" },
            { "efexor", "snri" },
            { "duloxetina", "snri" },
            { "duloxetine", "snri" },
            { "cymbalta", "snri" },

            // HCV
            { "glecaprevir", "glecaprevir/pibrentasvir" },
            { "pibrentasvir", "glecaprevir/pibrentasvir" },
            { "maviret", "glecaprevir/pibrentasvir" },

            // Altri
            { "digossina", "digossina" },
            { "digoxin", "digossina" },
            { "lanoxin", "digossina" },
            { "pantoprazolo", "pantoprazolo" },
            { "pantoprazole", "pantoprazolo" },
            { "omeprazolo", "omeprazolo" },
            { "omeprazole", "omeprazolo" },
        };

        // Cerca corrispondenza esatta
        if (synonyms.TryGetValue(normalizedDrug, out var mapped))
            return mapped;

        // Cerca corrispondenza parziale
        foreach (var kvp in synonyms)
        {
            if (normalizedDrug.Contains(kvp.Key) || kvp.Key.Contains(normalizedDrug))
                return kvp.Value;
        }

        return null;
    }

    public List<DOACInteraction> CheckMultipleInteractions(DOACType doacType, List<string> drugNames)
    {
        var interactions = new List<DOACInteraction>();

        foreach (var drugName in drugNames)
        {
            var interaction = CheckInteraction(doacType, drugName);
            if (interaction != null)
            {
                interactions.Add(interaction);
            }
        }

        return interactions.OrderByDescending(i => i.InteractionLevel).ToList();
    }

    private List<DOACInteraction> InitializeInteractions()
    {
        var interactions = new List<DOACInteraction>();

        // ========== DABIGATRAN ==========

        // Inibitori forti P-gp - CONTROINDICATI
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Inibitore forte P-gp",
            DrugName = "Ketoconazolo sistemico",
            InteractionLevel = DOACInteractionLevel.Contraindicated,
            Effect = "↑ AUC/Cmax ~2.4x",
            ClinicalRecommendation = "CONTROINDICATO",
            Notes = "Rischio emorragico significativamente aumentato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Inibitore forte P-gp",
            DrugName = "Ciclosporina",
            InteractionLevel = DOACInteractionLevel.Contraindicated,
            Effect = "↑ esposizione significativa",
            ClinicalRecommendation = "CONTROINDICATO"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Inibitore forte P-gp",
            DrugName = "Itraconazolo",
            InteractionLevel = DOACInteractionLevel.Contraindicated,
            Effect = "↑ esposizione (simile ketoconazolo)",
            ClinicalRecommendation = "CONTROINDICATO"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Inibitore forte P-gp",
            DrugName = "Dronedarone",
            InteractionLevel = DOACInteractionLevel.Contraindicated,
            Effect = "↑ AUC/Cmax ~2.4/2.3x",
            ClinicalRecommendation = "CONTROINDICATO"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Antivirale HCV",
            DrugName = "Glecaprevir/pibrentasvir",
            InteractionLevel = DOACInteractionLevel.Contraindicated,
            Effect = "↑ esposizione, ↑ rischio sanguinamento",
            ClinicalRecommendation = "CONTROINDICATO"
        });

        // Inibitori moderati P-gp - RIDUZIONE DOSE
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Inibitore moderato P-gp",
            DrugName = "Verapamil",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ Cmax/AUC fino a 2.8/2.5x",
            ClinicalRecommendation = "Riduzione dose (150 mg/die vs 220 mg/die in ortopedia); cautela con IR moderata",
            Notes = "Rischio emorragico aumentato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Inibitore moderato P-gp",
            DrugName = "Amiodarone",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC/Cmax ~1.6/1.5x; effetto persistente (lunga emivita)",
            ClinicalRecommendation = "Riduzione dose in ortopedia; cautela",
            Notes = "Monitorare per segni di sanguinamento"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Inibitore moderato P-gp",
            DrugName = "Chinidina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC/Cmax ~1.5x",
            ClinicalRecommendation = "Riduzione dose in ortopedia; stretto controllo clinico"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Antipiastrinico",
            DrugName = "Ticagrelor",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC/Cmax ~1.5-2x; interazione farmacodinamica",
            ClinicalRecommendation = "Cautela; ↑ rischio sanguinamento",
            Notes = "Doppia azione: farmacocinetica e farmacodinamica"
        });

        // Induttori P-gp - EVITARE
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Induttore P-gp",
            DrugName = "Rifampicina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione ~67%",
            ClinicalRecommendation = "EVITARE (↓ efficacia)",
            Notes = "Rischio trombotico significativamente aumentato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Induttore P-gp",
            DrugName = "Erba di San Giovanni",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione",
            ClinicalRecommendation = "EVITARE"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Induttore P-gp",
            DrugName = "Carbamazepina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione",
            ClinicalRecommendation = "EVITARE"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Induttore P-gp",
            DrugName = "Fenitoina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione",
            ClinicalRecommendation = "EVITARE"
        });

        // Antipiastrinici
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Antipiastrinico",
            DrugName = "ASA (acido acetilsalicilico)",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Interazione farmacodinamica",
            ClinicalRecommendation = "↑ rischio sanguinamento 12-24%; cautela",
            Notes = "Dose-dipendente; valutare attentamente indicazione"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Antipiastrinico",
            DrugName = "Clopidogrel",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC dabigatran 30-40% (con dose carico)",
            ClinicalRecommendation = "Cautela; ↑ rischio sanguinamento"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "FANS",
            DrugName = "FANS (naprossene, ibuprofene)",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Interazione farmacodinamica",
            ClinicalRecommendation = "↑ rischio ~50%; cautela (ok breve termine)",
            Notes = "Uso cronico sconsigliato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Antidepressivo",
            DrugName = "SSRI/SNRI",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Effetto piastrinico additivo",
            ClinicalRecommendation = "↑ rischio sanguinamento; cautela"
        });

        // Senza interazione significativa
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "Cardiotonico",
            DrugName = "Digossina",
            InteractionLevel = DOACInteractionLevel.None,
            Effect = "Nessun effetto",
            ClinicalRecommendation = "Nessuna modifica dose"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Dabigatran,
            DrugClass = "PPI",
            DrugName = "Pantoprazolo",
            InteractionLevel = DOACInteractionLevel.None,
            Effect = "Riduzione assorbimento non clinicamente rilevante",
            ClinicalRecommendation = "Nessuna modifica dose"
        });

        // ========== RIVAROXABAN ==========

        // Inibitori forti P-gp + CYP3A4 - NON RACCOMANDATO
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Inibitore forte P-gp + CYP3A4",
            DrugName = "Ketoconazolo",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC 2.6x, Cmax 1.7x",
            ClinicalRecommendation = "NON raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Inibitore forte P-gp + CYP3A4",
            DrugName = "Itraconazolo",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ esposizione significativa",
            ClinicalRecommendation = "NON raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Inibitore forte P-gp + CYP3A4",
            DrugName = "Voriconazolo",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ esposizione significativa",
            ClinicalRecommendation = "NON raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Inibitore forte P-gp + CYP3A4",
            DrugName = "Posaconazolo",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ esposizione significativa",
            ClinicalRecommendation = "NON raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Inibitore proteasi HIV",
            DrugName = "Ritonavir",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC 2.5x, Cmax 1.6x",
            ClinicalRecommendation = "NON raccomandato"
        });

        // Inibitori moderati - CAUTELA
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Inibitore moderato",
            DrugName = "Claritromicina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC 1.5x, Cmax 1.4x",
            ClinicalRecommendation = "Cautela; potenzialmente significativo in pz ad alto rischio",
            Notes = "Particolarmente con IR moderata"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Inibitore moderato",
            DrugName = "Eritromicina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC 1.3x; additivo con IR (1.8-2x con IR lieve-moderata)",
            ClinicalRecommendation = "Cautela; significativo con IR",
            Notes = "Con insufficienza renale l'interazione diventa clinicamente rilevante"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Antiaritmico",
            DrugName = "Dronedarone",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Dati limitati",
            ClinicalRecommendation = "EVITARE"
        });

        // Induttori forti CYP3A4 - EVITARE
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Induttore forte CYP3A4",
            DrugName = "Rifampicina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ AUC ~50%",
            ClinicalRecommendation = "EVITARE (↓ efficacia); monitorare segni trombosi se uso concomitante",
            Notes = "Rischio trombotico"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Induttore forte CYP3A4",
            DrugName = "Fenitoina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione",
            ClinicalRecommendation = "EVITARE"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Induttore forte CYP3A4",
            DrugName = "Carbamazepina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione",
            ClinicalRecommendation = "EVITARE"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Induttore forte CYP3A4",
            DrugName = "Fenobarbital",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione",
            ClinicalRecommendation = "EVITARE"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Induttore forte CYP3A4",
            DrugName = "Erba di San Giovanni",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione",
            ClinicalRecommendation = "EVITARE"
        });

        // Antipiastrinici
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Antipiastrinico",
            DrugName = "ASA (75-100 mg)",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Interazione farmacodinamica",
            ClinicalRecommendation = "Ok in associazione (studi COMPASS, ATLAS ACS-TIMI 46); ↑ rischio sanguinamento",
            Notes = "Studiato in pazienti con CAD/PAD o SCA"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Antipiastrinico",
            DrugName = "Clopidogrel (75 mg)",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "No interazione PK; ↑ tempo sanguinamento in sottogruppo",
            ClinicalRecommendation = "Ok (studi SCA); cautela",
            Notes = "Variabilità individuale"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Antipiastrinico",
            DrugName = "Prasugrel",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Non studiato",
            ClinicalRecommendation = "NON raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Antipiastrinico",
            DrugName = "Ticagrelor",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Non studiato",
            ClinicalRecommendation = "NON raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "FANS",
            DrugName = "FANS, SSRI/SNRI",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Interazione farmacodinamica",
            ClinicalRecommendation = "Cautela; ↑ rischio sanguinamento",
            Notes = "Uso cronico sconsigliato"
        });

        // Senza interazione
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Rivaroxaban,
            DrugClass = "Vari",
            DrugName = "Midazolam, Digossina, Atorvastatina, Omeprazolo",
            InteractionLevel = DOACInteractionLevel.None,
            Effect = "Nessun effetto",
            ClinicalRecommendation = "Nessuna modifica dose"
        });

        // ========== APIXABAN ==========

        // Inibitori forti P-gp + CYP3A4 - NON RACCOMANDATO
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Inibitore forte P-gp + CYP3A4",
            DrugName = "Ketoconazolo",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC 2x, Cmax 1.6x",
            ClinicalRecommendation = "NON raccomandato (specialmente con IR severa o altri fattori di rischio)"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Inibitore forte P-gp + CYP3A4",
            DrugName = "Itraconazolo",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ esposizione ~2x",
            ClinicalRecommendation = "NON raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Inibitore forte P-gp + CYP3A4",
            DrugName = "Voriconazolo",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ esposizione ~2x",
            ClinicalRecommendation = "NON raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Inibitore forte P-gp + CYP3A4",
            DrugName = "Posaconazolo",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ esposizione ~2x",
            ClinicalRecommendation = "NON raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Inibitore proteasi HIV",
            DrugName = "Ritonavir",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ esposizione ~2x",
            ClinicalRecommendation = "NON raccomandato"
        });

        // Inibitori moderati/deboli - NO AGGIUSTAMENTO
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Inibitore moderato CYP3A4",
            DrugName = "Diltiazem (360 mg/die)",
            InteractionLevel = DOACInteractionLevel.None,
            Effect = "↑ AUC 1.4x, Cmax 1.3x",
            ClinicalRecommendation = "Nessuna modifica dose"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Inibitore P-gp",
            DrugName = "Naprossene (500 mg)",
            InteractionLevel = DOACInteractionLevel.None,
            Effect = "↑ AUC 1.5x, Cmax 1.6x",
            ClinicalRecommendation = "Nessuna modifica dose; cautela (alcuni individui risposta pronunciata)"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Antibiotico macrolide",
            DrugName = "Claritromicina (500 mg bid)",
            InteractionLevel = DOACInteractionLevel.None,
            Effect = "↑ AUC 1.6x, Cmax 1.3x",
            ClinicalRecommendation = "Nessuna modifica dose"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Inibitori moderati",
            DrugName = "Amiodarone, Chinidina, Verapamil, Fluconazolo",
            InteractionLevel = DOACInteractionLevel.None,
            Effect = "↑ esposizione minore",
            ClinicalRecommendation = "Nessuna modifica dose"
        });

        // Induttori forti - CAUTELA O CONTROINDICATO
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Induttore forte P-gp + CYP3A4",
            DrugName = "Rifampicina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ AUC 54%, Cmax 42%",
            ClinicalRecommendation = "Trattamento TVP/EP: NON USARE (efficacia compromessa). Prevenzione ictus FANV: CAUTELA",
            Notes = "Riduzione efficacia e paradossalmente ↑ sanguinamento osservato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Induttore forte",
            DrugName = "Fenitoina, Carbamazepina, Fenobarbital, Erba di San Giovanni",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione ~50%",
            ClinicalRecommendation = "Come rifampicina"
        });

        // Antipiastrinici
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Antipiastrinico",
            DrugName = "ASA (325 mg)",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "No interazione PK/PD; ↑ sanguinamento maggiore 1.8→3.4%/anno (ARISTOTLE)",
            ClinicalRecommendation = "Cautela; valutare beneficio/rischio",
            Notes = "Dose-dipendente; alte dosi ASA aumentano significativamente rischio"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Doppia antipiastrinica",
            DrugName = "ASA + Doppia antipiastrinica",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ sanguinamento maggiore/CRNM 16.4→33.1%/anno (AUGUSTUS)",
            ClinicalRecommendation = "Attenta valutazione rischio/beneficio; cautela"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Antipiastrinico",
            DrugName = "Clopidogrel (75 mg) ± ASA",
            InteractionLevel = DOACInteractionLevel.Minor,
            Effect = "No ↑ tempo sanguinamento o inibizione piastrinica extra",
            ClinicalRecommendation = "Cautela; alcuni individui risposta pronunciata"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "FANS, antidepressivi",
            DrugName = "FANS, SSRI/SNRI",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Interazione farmacodinamica",
            ClinicalRecommendation = "Cautela; ↑ rischio sanguinamento",
            Notes = "Specialmente uso cronico"
        });

        // Senza interazione
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Apixaban,
            DrugClass = "Vari",
            DrugName = "Atenololo, Famotidina, Digossina",
            InteractionLevel = DOACInteractionLevel.None,
            Effect = "Nessun effetto",
            ClinicalRecommendation = "Nessuna modifica dose"
        });

        // ========== EDOXABAN ==========

        // Inibitori P-gp - RIDUZIONE DOSE 30 mg/die
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Inibitore P-gp",
            DrugName = "Ciclosporina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC 73%, Cmax 74%",
            ClinicalRecommendation = "Riduzione dose a 30 mg una volta/die"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Inibitore P-gp",
            DrugName = "Dronedarone",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC 85%, Cmax 46%",
            ClinicalRecommendation = "Riduzione dose a 30 mg una volta/die"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Inibitore P-gp",
            DrugName = "Eritromicina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC 85%, Cmax 68%",
            ClinicalRecommendation = "Riduzione dose a 30 mg una volta/die"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Inibitore P-gp",
            DrugName = "Ketoconazolo",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ AUC 87%, Cmax 89%",
            ClinicalRecommendation = "Riduzione dose a 30 mg una volta/die"
        });

        // Inibitori P-gp - DOSE 60 mg/die, NO RIDUZIONE
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Inibitore P-gp",
            DrugName = "Chinidina",
            InteractionLevel = DOACInteractionLevel.Minor,
            Effect = "↑ AUC 77%, Cmax 85%",
            ClinicalRecommendation = "Dose 60 mg/die; no riduzione"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Inibitore P-gp",
            DrugName = "Verapamil",
            InteractionLevel = DOACInteractionLevel.Minor,
            Effect = "↑ AUC/Cmax ~53%",
            ClinicalRecommendation = "Dose 60 mg/die; no riduzione"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Inibitore P-gp",
            DrugName = "Amiodarone",
            InteractionLevel = DOACInteractionLevel.Minor,
            Effect = "↑ AUC 40%, Cmax 66%",
            ClinicalRecommendation = "Dose 60 mg/die; no riduzione (non clinicamente significativo in ENGAGE AF-TIMI 48)"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Antibiotico macrolide",
            DrugName = "Claritromicina",
            InteractionLevel = DOACInteractionLevel.Minor,
            Effect = "↑ AUC 53%, Cmax 27%",
            ClinicalRecommendation = "Dose 60 mg/die; no riduzione"
        });

        // Induttori P-gp - CAUTELA
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Induttore P-gp",
            DrugName = "Rifampicina",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ AUC ed emivita",
            ClinicalRecommendation = "Usare con cautela (↓ efficacia)",
            Notes = "Rischio trombotico"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Induttore P-gp",
            DrugName = "Fenitoina, Carbamazepina, Fenobarbital, Iperico",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↓ esposizione",
            ClinicalRecommendation = "Usare con cautela"
        });

        // Antipiastrinici
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Antipiastrinico",
            DrugName = "ASA ≤100 mg",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ tempo sanguinamento; no effetto su Cmax/AUC",
            ClinicalRecommendation = "Ok; ↑ sanguinamento maggiore 2x vs no antipiastrinici (ENGAGE AF-TIMI 48)",
            Notes = "Combinazione aumenta rischio ma clinicamente accettabile con indicazione appropriata"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Antipiastrinico",
            DrugName = "ASA 325 mg (alte dosi)",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ Cmax/AUC edoxaban ~35-32%; ↑ tempo sanguinamento",
            ClinicalRecommendation = "NON raccomandato co-somministrazione cronica; solo sotto supervisione medica"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Tienopiridine",
            DrugName = "Clopidogrel",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ sanguinamento clinicamente rilevante",
            ClinicalRecommendation = "Cautela; minore rischio vs warfarin"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "FANS",
            DrugName = "Naprossene",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "↑ tempo sanguinamento; no effetto Cmax/AUC",
            ClinicalRecommendation = "Uso cronico non raccomandato"
        });

        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Antidepressivo",
            DrugName = "SSRI/SNRI",
            InteractionLevel = DOACInteractionLevel.Dangerous,
            Effect = "Effetto piastrinico additivo",
            ClinicalRecommendation = "Cautela; ↑ rischio sanguinamento"
        });

        // Senza interazione
        interactions.Add(new DOACInteraction
        {
            DOACType = DOACType.Edoxaban,
            DrugClass = "Cardiotonico",
            DrugName = "Digossina",
            InteractionLevel = DOACInteractionLevel.None,
            Effect = "↑ Cmax digossina 28%; no effetto AUC",
            ClinicalRecommendation = "No modifica dose; non clinicamente rilevante"
        });

        return interactions;
    }
}
