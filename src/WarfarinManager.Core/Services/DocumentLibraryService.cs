using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WarfarinManager.Core.Services;

/// <summary>
/// Categoria di documenti
/// </summary>
public enum DocumentCategory
{
    Warfarin,
    DOAC,
    LineGuida,
    Switch,
    PerPazienti,
    Riferimenti
}

/// <summary>
/// Rappresenta un documento nella libreria
/// </summary>
public class DocumentInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DocumentCategory Category { get; set; }
    public string Icon { get; set; } = "📄";
    public bool IsPdf { get; set; }
    public bool IsHtml { get; set; }
    public string[] SearchKeywords { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Interfaccia per il servizio di libreria documenti
/// </summary>
public interface IDocumentLibraryService
{
    IEnumerable<DocumentInfo> GetAllDocuments();
    IEnumerable<DocumentInfo> GetDocumentsByCategory(DocumentCategory category);
    IEnumerable<DocumentInfo> SearchDocuments(string searchText);
    string GetDocumentPath(DocumentInfo document);
    void OpenDocument(DocumentInfo document);
}

/// <summary>
/// Servizio per la gestione della libreria documenti
/// </summary>
public class DocumentLibraryService : IDocumentLibraryService
{
    private readonly List<DocumentInfo> _documents;
    private readonly string _guidesBasePath;

    public DocumentLibraryService()
    {
        _guidesBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Guides");
        _documents = InitializeDocuments();
    }

    private List<DocumentInfo> InitializeDocuments()
    {
        return new List<DocumentInfo>
        {
            // ====== WARFARIN ======
            new DocumentInfo
            {
                Id = "warfarin-interactions",
                Title = "Interazioni Farmacologiche Warfarin",
                Description = "Guida completa alle interazioni farmacologiche del Warfarin con altri farmaci",
                FileName = "interactions.html",
                Category = DocumentCategory.Warfarin,
                Icon = "💊",
                IsHtml = true,
                SearchKeywords = new[] { "interazioni", "farmaci", "warfarin", "cyp450", "vitamina k" }
            },
            new DocumentInfo
            {
                Id = "inr-flowchart",
                Title = "Flowchart Gestione INR",
                Description = "Algoritmo decisionale interattivo per la gestione dei valori INR fuori range",
                FileName = "algoritmo-gestione-inr.html",
                Category = DocumentCategory.Warfarin,
                Icon = "📊",
                IsHtml = true,
                SearchKeywords = new[] { "inr", "algoritmo", "flowchart", "gestione", "sovraterapeutico", "sottoterapeutico" }
            },
            new DocumentInfo
            {
                Id = "tao-infographic",
                Title = "Infografica Gestione TAO",
                Description = "Visualizzazione grafica delle informazioni chiave per la gestione della TAO",
                FileName = "infografica-tao.html",
                Category = DocumentCategory.Warfarin,
                Icon = "📋",
                IsHtml = true,
                SearchKeywords = new[] { "infografica", "tao", "schema", "visuale" }
            },
            new DocumentInfo
            {
                Id = "linee-guida-tao-html",
                Title = "Guida TAO per MMG (Testo)",
                Description = "Guida completa alla terapia anticoagulante orale con Warfarin - versione HTML navigabile",
                FileName = "linee-guida-tao.html",
                Category = DocumentCategory.Warfarin,
                Icon = "📄",
                IsHtml = true,
                SearchKeywords = new[] { "guida", "mmg", "tao", "warfarin", "terapia" }
            },
            new DocumentInfo
            {
                Id = "linee-guida-tao-pdf",
                Title = "Guida TAO per MMG (PDF)",
                Description = "Guida completa alla terapia anticoagulante orale con Warfarin - versione PDF stampabile",
                FileName = "LineeGuida.pdf",
                Category = DocumentCategory.Warfarin,
                Icon = "📕",
                IsPdf = true,
                SearchKeywords = new[] { "guida", "mmg", "tao", "warfarin", "pdf", "stampa" }
            },

            // ====== DOAC ======
            new DocumentInfo
            {
                Id = "doac-interactions-checker",
                Title = "Checker Interazioni DOAC",
                Description = "Tabella interattiva delle interazioni farmacologiche dei DOAC con P-gp e CYP3A4",
                FileName = "DoacsInteractions.html",
                Category = DocumentCategory.DOAC,
                Icon = "🔍",
                IsHtml = true,
                SearchKeywords = new[] { "doac", "interazioni", "pgp", "cyp3a4", "checker" }
            },
            new DocumentInfo
            {
                Id = "rcp-apixaban",
                Title = "RCP Apixaban (Eliquis)",
                Description = "Riassunto delle Caratteristiche del Prodotto - Apixaban (EMA)",
                FileName = "RCP_apixaban.pdf",
                Category = DocumentCategory.DOAC,
                Icon = "📕",
                IsPdf = true,
                SearchKeywords = new[] { "apixaban", "eliquis", "rcp", "scheda tecnica", "ema" }
            },
            new DocumentInfo
            {
                Id = "rcp-rivaroxaban",
                Title = "RCP Rivaroxaban (Xarelto)",
                Description = "Riassunto delle Caratteristiche del Prodotto - Rivaroxaban (EMA)",
                FileName = "RCP_rivaroxaban.pdf",
                Category = DocumentCategory.DOAC,
                Icon = "📕",
                IsPdf = true,
                SearchKeywords = new[] { "rivaroxaban", "xarelto", "rcp", "scheda tecnica", "ema" }
            },
            new DocumentInfo
            {
                Id = "rcp-dabigatran",
                Title = "RCP Dabigatran (Pradaxa)",
                Description = "Riassunto delle Caratteristiche del Prodotto - Dabigatran (EMA)",
                FileName = "RCP_dabigatran.pdf",
                Category = DocumentCategory.DOAC,
                Icon = "📕",
                IsPdf = true,
                SearchKeywords = new[] { "dabigatran", "pradaxa", "rcp", "scheda tecnica", "ema" }
            },
            new DocumentInfo
            {
                Id = "rcp-edoxaban",
                Title = "RCP Edoxaban (Lixiana)",
                Description = "Riassunto delle Caratteristiche del Prodotto - Edoxaban (EMA)",
                FileName = "RCP_edoxaban.pdf",
                Category = DocumentCategory.DOAC,
                Icon = "📕",
                IsPdf = true,
                SearchKeywords = new[] { "edoxaban", "lixiana", "rcp", "scheda tecnica", "ema" }
            },

            // ====== SWITCH ======
            new DocumentInfo
            {
                Id = "switch-infographic",
                Title = "Infografica Switch DOAC ↔ Warfarin",
                Description = "Schema visuale interattivo per lo switch bidirezionale tra DOAC e anticoagulanti cumarolici",
                FileName = "guida-switch-infografica.html",
                Category = DocumentCategory.Switch,
                Icon = "🔄",
                IsHtml = true,
                SearchKeywords = new[] { "switch", "doac", "warfarin", "infografica", "schema" }
            },
            new DocumentInfo
            {
                Id = "switch-guide-pdf",
                Title = "Guida Switch DOAC ↔ Warfarin (PDF)",
                Description = "Protocollo dettagliato per lo switch bidirezionale - versione PDF stampabile",
                FileName = "Switch_Doac_Warf.pdf",
                Category = DocumentCategory.Switch,
                Icon = "📕",
                IsPdf = true,
                SearchKeywords = new[] { "switch", "doac", "warfarin", "protocollo", "pdf" }
            },

            // ====== LINEE GUIDA ======
            new DocumentInfo
            {
                Id = "lg-fcsa-simg-2018",
                Title = "Linee Guida FCSA-SIMG 2018",
                Description = "Linee guida congiunte FCSA-SIMG sulla gestione della TAO",
                FileName = "lgFCSA-SIMG-2018.pdf",
                Category = DocumentCategory.LineGuida,
                Icon = "📚",
                IsPdf = true,
                SearchKeywords = new[] { "fcsa", "simg", "linee guida", "2018", "tao" }
            },
            new DocumentInfo
            {
                Id = "lg-fcsa",
                Title = "Linee Guida FCSA",
                Description = "Linee guida FCSA sulla terapia anticoagulante orale",
                FileName = "lgFCSA.pdf",
                Category = DocumentCategory.LineGuida,
                Icon = "📚",
                IsPdf = true,
                SearchKeywords = new[] { "fcsa", "linee guida", "tao" }
            },
            new DocumentInfo
            {
                Id = "lg-esc-2024",
                Title = "Linee Guida ESC 2024 - Raffronto",
                Description = "Confronto tra le linee guida ESC 2024 e altre linee guida internazionali",
                FileName = "lgESC2024-raffronto-con-altre.pdf",
                Category = DocumentCategory.LineGuida,
                Icon = "📚",
                IsPdf = true,
                SearchKeywords = new[] { "esc", "2024", "linee guida", "raffronto", "confronto" }
            },

            // ====== PER PAZIENTI ======
            new DocumentInfo
            {
                Id = "patient-guide",
                Title = "Guida Warfarin per Pazienti",
                Description = "Informazioni per i pazienti in terapia con Warfarin - PDF stampabile",
                FileName = "Guida Warfarin per pazienti.pdf",
                Category = DocumentCategory.PerPazienti,
                Icon = "👥",
                IsPdf = true,
                SearchKeywords = new[] { "pazienti", "guida", "warfarin", "informazioni" }
            },

            // ====== RIFERIMENTI ======
            new DocumentInfo
            {
                Id = "nomogramma-pengo",
                Title = "Nomogramma di Pengo",
                Description = "Nomogramma per la stima del dosaggio iniziale di Warfarin",
                FileName = "nomogramma-pengo.html",
                Category = DocumentCategory.Riferimenti,
                Icon = "📐",
                IsHtml = true,
                SearchKeywords = new[] { "nomogramma", "pengo", "dosaggio", "iniziale" }
            },
            new DocumentInfo
            {
                Id = "bridge-therapy",
                Title = "Bridge Therapy - Differenze FCSA-ACCP",
                Description = "Confronto tra le raccomandazioni FCSA e ACCP sulla bridge therapy perioperatoria",
                FileName = "Bridge therapia per TAO - differenze FCSA-ACCP.pdf",
                Category = DocumentCategory.Riferimenti,
                Icon = "🏥",
                IsPdf = true,
                SearchKeywords = new[] { "bridge", "therapy", "perioperatorio", "fcsa", "accp" }
            },
            new DocumentInfo
            {
                Id = "adverse-reactions",
                Title = "Reazioni Avverse Warfarin",
                Description = "Guida alle reazioni avverse e gestione delle complicanze della terapia con Warfarin",
                FileName = "Reazioni Avverse Warfarin - PDF.pdf",
                Category = DocumentCategory.Riferimenti,
                Icon = "⚠️",
                IsPdf = true,
                SearchKeywords = new[] { "reazioni", "avverse", "warfarin", "complicanze", "emorragia" }
            }
        };
    }

    public IEnumerable<DocumentInfo> GetAllDocuments()
    {
        return _documents.OrderBy(d => d.Category).ThenBy(d => d.Title);
    }

    public IEnumerable<DocumentInfo> GetDocumentsByCategory(DocumentCategory category)
    {
        return _documents.Where(d => d.Category == category).OrderBy(d => d.Title);
    }

    public IEnumerable<DocumentInfo> SearchDocuments(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return GetAllDocuments();

        var searchTerms = searchText.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return _documents.Where(d =>
            searchTerms.All(term =>
                d.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                d.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                d.SearchKeywords.Any(k => k.Contains(term, StringComparison.OrdinalIgnoreCase))
            )
        ).OrderByDescending(d => GetRelevanceScore(d, searchTerms));
    }

    private int GetRelevanceScore(DocumentInfo doc, string[] searchTerms)
    {
        int score = 0;
        foreach (var term in searchTerms)
        {
            if (doc.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
                score += 10;
            if (doc.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
                score += 5;
            if (doc.SearchKeywords.Any(k => k.Equals(term, StringComparison.OrdinalIgnoreCase)))
                score += 8;
            if (doc.SearchKeywords.Any(k => k.Contains(term, StringComparison.OrdinalIgnoreCase)))
                score += 3;
        }
        return score;
    }

    public string GetDocumentPath(DocumentInfo document)
    {
        return Path.Combine(_guidesBasePath, document.FileName);
    }

    public void OpenDocument(DocumentInfo document)
    {
        var path = GetDocumentPath(document);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Documento non trovato: {document.Title}", path);
        }

        var processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(processStartInfo);
    }

    /// <summary>
    /// Ottiene il nome della categoria per la visualizzazione
    /// </summary>
    public static string GetCategoryDisplayName(DocumentCategory category)
    {
        return category switch
        {
            DocumentCategory.Warfarin => "📋 Warfarin / TAO",
            DocumentCategory.DOAC => "💊 DOAC (NAO)",
            DocumentCategory.LineGuida => "📚 Linee Guida",
            DocumentCategory.Switch => "🔄 Switch Terapia",
            DocumentCategory.PerPazienti => "👥 Per i Pazienti",
            DocumentCategory.Riferimenti => "📖 Riferimenti",
            _ => category.ToString()
        };
    }
}
