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
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "0.5-1.5%", canOccurAtStart: false)]
        IntracranialHemorrhage = 100,

        /// <summary>
        /// Emorragia maggiore (6.0-7.2%)
        /// </summary>
        [Description("Emorragia maggiore")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "6.0-7.2%", canOccurAtStart: true)]
        MajorHemorrhage = 101,

        /// <summary>
        /// Emorragia fatale (0.67-1.3%)
        /// </summary>
        [Description("Emorragia fatale")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "0.67-1.3%", canOccurAtStart: false)]
        FatalHemorrhage = 102,

        /// <summary>
        /// Sanguinamento gastrointestinale (2.5-3.5%, 35% emorragie maggiori)
        /// </summary>
        [Description("Sanguinamento gastrointestinale")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "2.5-3.5%", canOccurAtStart: true)]
        GastrointestinalBleeding = 103,

        /// <summary>
        /// Sanguinamento retroperitoneale (0.5-1.0%, Shock ipovolemico)
        /// </summary>
        [Description("Sanguinamento retroperitoneale")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "0.5-1.0%", canOccurAtStart: false)]
        RetroperitonealBleeding = 104,

        /// <summary>
        /// Emottisi (0.3-0.5%, Insufficienza respiratoria)
        /// </summary>
        [Description("Emottisi")]
        [AdverseReactionInfo(AdverseReactionSeverity.Critical, incidence: "0.3-0.5%", canOccurAtStart: false)]
        Hemoptysis = 105,

        // ====================================
        // COMPLICAZIONI GRAVI (GIALLO)
        // ====================================

        /// <summary>
        /// Sanguinamento delle gengive (3-5%)
        /// </summary>
        [Description("Sanguinamento delle gengive")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "3-5%", canOccurAtStart: true)]
        GingivalBleeding = 200,

        /// <summary>
        /// Epistassi (3-5%)
        /// </summary>
        [Description("Epistassi")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "3-5%", canOccurAtStart: true)]
        Epistaxis = 201,

        /// <summary>
        /// Ematuria (2-4%)
        /// </summary>
        [Description("Ematuria")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "2-4%", canOccurAtStart: true)]
        Hematuria = 202,

        /// <summary>
        /// Sanguinamento genito-urinario (2-3%)
        /// </summary>
        [Description("Sanguinamento genito-urinario")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "2-3%", canOccurAtStart: false)]
        GenitourinaryBleeding = 203,

        /// <summary>
        /// Ematoma significativo (2-3%)
        /// </summary>
        [Description("Ematoma significativo")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "2-3%", canOccurAtStart: true)]
        SignificantHematoma = 204,

        /// <summary>
        /// Emorragia intra-articolare (emartro) (0.5-1.0%)
        /// </summary>
        [Description("Emorragia intra-articolare (emartro)")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "0.5-1.0%", canOccurAtStart: false)]
        IntraArticularHemorrhage = 205,

        /// <summary>
        /// Emorragia oculare/intra-oculare (0.3-0.5%)
        /// </summary>
        [Description("Emorragia oculare/intra-oculare")]
        [AdverseReactionInfo(AdverseReactionSeverity.Serious, incidence: "0.3-0.5%", canOccurAtStart: false)]
        OcularHemorrhage = 206,

        // ====================================
        // EFFETTI AVVERSI COMUNI (VERDE)
        // ====================================

        /// <summary>
        /// Nausea/Vomito (5-10%)
        /// </summary>
        [Description("Nausea/Vomito")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "5-10%", canOccurAtStart: true)]
        NauseaVomiting = 300,

        /// <summary>
        /// Diarrea/Costipazione (3-8%)
        /// </summary>
        [Description("Diarrea/Costipazione")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "3-8%", canOccurAtStart: true)]
        DiarrheaConstipation = 301,

        /// <summary>
        /// Cefalea (5-8%)
        /// </summary>
        [Description("Cefalea")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "5-8%", canOccurAtStart: true)]
        Headache = 302,

        /// <summary>
        /// Capogiri/Debolezza (4-6%)
        /// </summary>
        [Description("Capogiri/Debolezza")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "4-6%", canOccurAtStart: true)]
        DizzinessWeakness = 303,

        /// <summary>
        /// Alterazione del gusto (2-5%)
        /// </summary>
        [Description("Alterazione del gusto")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "2-5%", canOccurAtStart: true)]
        TasteAlteration = 304,

        /// <summary>
        /// Gonfiore addominale (2-3%)
        /// </summary>
        [Description("Gonfiore addominale")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "2-3%", canOccurAtStart: true)]
        AbdominalSwelling = 305,

        /// <summary>
        /// Alopecia (0.5-1%)
        /// </summary>
        [Description("Alopecia")]
        [AdverseReactionInfo(AdverseReactionSeverity.Common, incidence: "0.5-1%", canOccurAtStart: false)]
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

        public AdverseReactionInfoAttribute(AdverseReactionSeverity severity, string incidence, bool canOccurAtStart)
        {
            Severity = severity;
            Incidence = incidence;
            CanOccurAtStart = canOccurAtStart;
        }
    }
}
