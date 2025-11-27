namespace WarfarinManager.Shared.Enums
{
    /// <summary>
    /// Livello di certezza della correlazione tra reazione avversa e terapia con Warfarin
    /// </summary>
    public enum CertaintyLevel
    {
        /// <summary>
        /// Correlazione sicura con il Warfarin
        /// </summary>
        Certain = 0,

        /// <summary>
        /// Correlazione dubbia o incerta
        /// </summary>
        Doubtful = 1
    }
}
