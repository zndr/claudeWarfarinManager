using System;
using System.ComponentModel;

namespace WarfarinManager.Shared.Enums
{
    /// <summary>
    /// Tipi di reazioni avverse al Warfarin classificate per gravità
    /// </summary>
    public enum AdverseReactionType
    {
        // ====================================
        // COMPLICAZIONI CRITICHE (ROSSO)
        // ====================================

        /// <summary>
        /// Emorragia intracranica (0.5-1.5%, Mortalità 20-30%)
        /// </summary>
        [Description("Emorragia intracranica")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "0.5-1.5%", canOccurAtStart: false,
            mechanism: "Eccessiva anticoagulazione → rottura vasi cerebrali fragili → accumulo sangue intracranico")]
        IntracranialHemorrhage = 100,

        /// <summary>
        /// Emorragia maggiore (6.0-7.2%)
        /// </summary>
        [Description("Emorragia maggiore")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "6.0-7.2%", canOccurAtStart: true,
            mechanism: "INR >4-5 → deficit multipli fattori coagulazione (II, VII, IX, X) → sanguinamento spontaneo")]
        MajorHemorrhage = 101,

        /// <summary>
        /// Emorragia fatale (0.67-1.3%)
        /// </summary>
        [Description("Emorragia fatale")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "0.67-1.3%", canOccurAtStart: false,
            mechanism: "Anticoagulazione estrema (INR >10) → emorragia massiva incontrollabile → shock ipovolemico")]
        FatalHemorrhage = 102,

        /// <summary>
        /// Sanguinamento gastrointestinale (2.5-3.5%, 35% emorragie maggiori)
        /// </summary>
        [Description("Sanguinamento gastrointestinale")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "2.5-3.5%", canOccurAtStart: true,
            mechanism: "Mucosa GI fragile + anticoagulazione → erosione/ulcerazione → sanguinamento attivo")]
        GastrointestinalBleeding = 103,

        /// <summary>
        /// Sanguinamento retroperitoneale (0.5-1.0%, Shock ipovolemico)
        /// </summary>
        [Description("Sanguinamento retroperitoneale")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "0.5-1.0%", canOccurAtStart: false,
            mechanism: "Trauma minore + anticoagulazione → rottura vasi retroperitoneali → accumulo ematico occulto")]
        RetroperitonealBleeding = 104,

        /// <summary>
        /// Emottisi (0.3-0.5%, Insufficienza respiratoria)
        /// </summary>
        [Description("Emottisi")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "0.3-0.5%", canOccurAtStart: false,
            mechanism: "Capillari alveolari fragili + anticoagulazione → rottura microvasale → sangue nelle vie aeree")]
        Hemoptysis = 105,

        /// <summary>
        /// Necrosi cutanea da warfarin (0.01-0.1%, Rischio 3-10 giorni dall'inizio)
        /// </summary>
        [Description("Necrosi Cutanea")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "0.01-0.1%", canOccurAtStart: true,
            mechanism: "Deficit transitorio Proteina C/S → microtrombosi venosa → ischemia tessutale → necrosi")]
        SkinNecrosis = 106,

        // ====================================
        // COMPLICAZIONI GRAVI (GIALLO)
        // ====================================

        /// <summary>
        /// Sanguinamento delle gengive (3-5%)
        /// </summary>
        [Description("Sanguinamento delle gengive")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "3-5%", canOccurAtStart: true,
            mechanism: "Mucosa gengivale vascolarizzata + anticoagulazione → sanguinamento spontaneo o post-spazzolamento")]
        GingivalBleeding = 200,

        /// <summary>
        /// Epistassi (3-5%)
        /// </summary>
        [Description("Epistassi")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "3-5%", canOccurAtStart: true,
            mechanism: "Plesso di Kiesselbach (mucosa nasale) + anticoagulazione → rottura capillari → epistassi")]
        Epistaxis = 201,

        /// <summary>
        /// Ematuria (2-4%)
        /// </summary>
        [Description("Ematuria")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "2-4%", canOccurAtStart: true,
            mechanism: "Filtrazione glomerulare alterata + anticoagulazione → passaggio eritrociti nelle urine")]
        Hematuria = 202,

        /// <summary>
        /// Sanguinamento genito-urinario (2-3%)
        /// </summary>
        [Description("Sanguinamento genito-urinario")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "2-3%", canOccurAtStart: false,
            mechanism: "Mucosa genito-urinaria + anticoagulazione → menorragia, metrorragia o ematuria macroscopica")]
        GenitourinaryBleeding = 203,

        /// <summary>
        /// Ematoma significativo (2-3%)
        /// </summary>
        [Description("Ematoma significativo")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "2-3%", canOccurAtStart: true,
            mechanism: "Trauma minore + anticoagulazione → accumulo ematico sottocutaneo/muscolare esteso")]
        SignificantHematoma = 204,

        /// <summary>
        /// Emorragia intra-articolare (emartro) (0.5-1.0%)
        /// </summary>
        [Description("Emorragia intra-articolare (emartro)")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "0.5-1.0%", canOccurAtStart: false,
            mechanism: "Membrana sinoviale vascolare + anticoagulazione → sanguinamento intrarticolare → emartro")]
        IntraArticularHemorrhage = 205,

        /// <summary>
        /// Emorragia oculare/intra-oculare (0.3-0.5%)
        /// </summary>
        [Description("Emorragia oculare/intra-oculare")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "0.3-0.5%", canOccurAtStart: false,
            mechanism: "Capillari retinici/congiuntivali + anticoagulazione → emorragia vitrea/retinica/ipo ftalmica")]
        OcularHemorrhage = 206,

        // ====================================
        // EFFETTI AVVERSI COMUNI (VERDE)
        // ====================================

        /// <summary>
        /// Nausea/Vomito (5-10%)
        /// </summary>
        [Description("Nausea/Vomito")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "5-10%", canOccurAtStart: true,
            mechanism: "Irritazione mucosa gastrica + stimolazione chemocettori zona trigger → nausea/vomito")]
        NauseaVomiting = 300,

        /// <summary>
        /// Diarrea/Costipazione (3-8%)
        /// </summary>
        [Description("Diarrea/Costipazione")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "3-8%", canOccurAtStart: true,
            mechanism: "Alterazione flora intestinale + motilità GI → diarrea o costipazione")]
        DiarrheaConstipation = 301,

        /// <summary>
        /// Cefalea (5-8%)
        /// </summary>
        [Description("Cefalea")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "5-8%", canOccurAtStart: true,
            mechanism: "Alterazione microcircolo cerebrale + vasodilatazione → cefalea vasomotoria")]
        Headache = 302,

        /// <summary>
        /// Capogiri/Debolezza (4-6%)
        /// </summary>
        [Description("Capogiri/Debolezza")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "4-6%", canOccurAtStart: true,
            mechanism: "Microemorragie transitorie + alterato flusso ematico → astenia e vertigini")]
        DizzinessWeakness = 303,

        /// <summary>
        /// Alterazione del gusto (2-5%)
        /// </summary>
        [Description("Alterazione del gusto")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "2-5%", canOccurAtStart: true,
            mechanism: "Interferenza con recettori gustativi + metabolismo zinco → disgeusia")]
        TasteAlteration = 304,

        /// <summary>
        /// Gonfiore addominale (2-3%)
        /// </summary>
        [Description("Gonfiore addominale")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "2-3%", canOccurAtStart: true,
            mechanism: "Irritazione mucosa GI + rallentata motilità intestinale → meteorismo e distensione")]
        AbdominalSwelling = 305,

        /// <summary>
        /// Alopecia (0.5-1%)
        /// </summary>
        [Description("Alopecia")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "0.5-1%", canOccurAtStart: false,
            mechanism: "Inibizione divisione cellulare follicoli piliferi → telogen effluvium (reversibile)")]
        Alopecia = 306
    }

    /// <summary>
    /// Attributo per metadati sulle reazioni avverse
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AdverseReactionInfoAttribute : Attribute
    {
        public AdverseReactionSeverity Severity { get; }
        public string Incidence { get; }
        public bool CanOccurAtStart { get; }
        public string? Mechanism { get; }

        public AdverseReactionInfoAttribute(AdverseReactionSeverity severity, string incidence, bool canOccurAtStart, string? mechanism = null)
        {
            Severity = severity;
            Incidence = incidence;
            CanOccurAtStart = canOccurAtStart;
            Mechanism = mechanism;
        }
    }
}
