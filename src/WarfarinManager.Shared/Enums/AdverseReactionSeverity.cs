namespace WarfarinManager.Shared.Enums
{
    /// <summary>
    /// Livelli di gravità delle reazioni avverse al Warfarin
    /// </summary>
    public enum AdverseReactionSeverity
    {
        /// <summary>
        /// COMPLICAZIONI CRITICHE (ROSSO) - Alto rischio, possibile mortalità/ospedalizzazione
        /// </summary>
        Critical = 0,

        /// <summary>
        /// COMPLICAZIONI GRAVI (GIALLO) - Richiedono attenzione medica
        /// </summary>
        Serious = 1,

        /// <summary>
        /// EFFETTI AVVERSI COMUNI (VERDE) - Comuni, non emorragici
        /// </summary>
        Common = 2
    }
}
