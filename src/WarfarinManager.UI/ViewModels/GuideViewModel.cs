using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace WarfarinManager.UI.ViewModels;

/// <summary>
/// ViewModel per la visualizzazione delle guide professionali
/// </summary>
public partial class GuideViewModel : ObservableObject
{
    private readonly ILogger<GuideViewModel> _logger;

    [ObservableProperty]
    private string _guidePath = string.Empty;

    [ObservableProperty]
    private string _guideTitle = string.Empty;

    public GuideViewModel(ILogger<GuideViewModel> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Inizializza il ViewModel con il percorso della guida
    /// </summary>
    public void Initialize(string guideName, string title)
    {
        try
        {
            GuideTitle = title;

            // Ottieni il percorso base dell'applicazione
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var guidesPath = Path.Combine(baseDirectory, "Resources", "Guides");

            // Costruisci il percorso completo del file HTML
            var fullPath = Path.Combine(guidesPath, guideName);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning($"Guida non trovata: {fullPath}");
                fullPath = Path.Combine(guidesPath, "index.html");
            }

            // WebView2 richiede un URI assoluto
            GuidePath = new Uri(fullPath).AbsoluteUri;

            _logger.LogInformation($"Guida caricata: {GuidePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'inizializzazione della guida");
            throw;
        }
    }
}
