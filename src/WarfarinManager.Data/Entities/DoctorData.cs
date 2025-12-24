namespace WarfarinManager.Data.Entities;

/// <summary>
/// Dati del medico che utilizza l'applicazione
/// </summary>
public class DoctorData : BaseEntity
{
    /// <summary>
    /// Cognome e nome del medico
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Codice fiscale del medico
    /// </summary>
    public string FiscalCode { get; set; } = string.Empty;

    /// <summary>
    /// Via e numero civico
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Codice di avviamento postale
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Città
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Numero di telefono
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Indirizzo email
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
