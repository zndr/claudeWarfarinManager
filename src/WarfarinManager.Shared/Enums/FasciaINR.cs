namespace WarfarinManager.Shared.Enums;

/// <summary>
/// Classificazione dettagliata dello stato INR rispetto al range terapeutico
/// Basato su linee guida FCSA-SIMG 2024
/// </summary>
public enum FasciaINR
{
    /// <summary>
    /// INR nel range terapeutico
    /// </summary>
    InRange = 0,

    // ===== INR SOTTOTERAPEUTICO =====

    /// <summary>
    /// INR lievemente basso
    /// - Target 2.0-3.0: INR 1.8-1.99
    /// - Target 2.5-3.5: INR 2.3-2.49
    /// </summary>
    SubLieve = 1,

    /// <summary>
    /// INR moderatamente basso
    /// - Target 2.0-3.0: INR 1.5-1.79
    /// - Target 2.5-3.5: INR 2.0-2.29
    /// </summary>
    SubModerato = 2,

    /// <summary>
    /// INR criticamente basso
    /// - Target 2.0-3.0: INR &lt;1.5
    /// - Target 2.5-3.5: INR &lt;2.0
    /// </summary>
    SubCritico = 3,

    // ===== INR SOVRATERAPEUTICO =====

    /// <summary>
    /// INR lievemente alto
    /// - Target 2.0-3.0: INR 3.1-3.4
    /// - Target 2.5-3.5: INR 3.6-3.9
    /// </summary>
    SovraLieve = 10,

    /// <summary>
    /// INR moderatamente alto
    /// - Target 2.0-3.0: INR 3.5-3.9
    /// - Target 2.5-3.5: INR 4.0-4.4
    /// </summary>
    SovraModerato = 11,

    /// <summary>
    /// INR alto
    /// - Target 2.0-3.0: INR 4.0-4.9
    /// - Target 2.5-3.5: INR 4.5-5.4
    /// </summary>
    SovraAlto = 12,

    /// <summary>
    /// INR molto alto
    /// - Target 2.0-3.0: INR 5.0-5.9
    /// - Target 2.5-3.5: INR 5.5-6.4
    /// </summary>
    SovraMoltoAlto = 13,

    /// <summary>
    /// INR critico
    /// - Target 2.0-3.0: INR 6.0-7.9
    /// - Target 2.5-3.5: INR 6.5-8.4
    /// </summary>
    SovraCritico = 14,

    /// <summary>
    /// INR estremo (rischio emorragico molto elevato)
    /// - Target 2.0-3.0: INR ≥8.0
    /// - Target 2.5-3.5: INR ≥8.5
    /// </summary>
    SovraEstremo = 15
}
