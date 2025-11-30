namespace WarfarinManager.Core.Models;

/// <summary>
/// Direzione dello switch terapeutico
/// </summary>
public enum SwitchDirection
{
    /// <summary>
    /// Passaggio da Warfarin/Acenocumarolo a DOAC
    /// </summary>
    WarfarinToDoac,

    /// <summary>
    /// Passaggio da DOAC a Warfarin/Acenocumarolo
    /// </summary>
    DoacToWarfarin
}
