using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using WarfarinManager.Core.Services;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.Dialogs;

public partial class DocumentBrowserDialog : Window
{
    private readonly IDocumentLibraryService _documentLibraryService;
    private readonly IServiceProvider _serviceProvider;
    private DocumentCategory? _currentCategory;
    private string _currentSearchText = string.Empty;
    private bool _isLoaded;

    public DocumentBrowserDialog(IDocumentLibraryService documentLibraryService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _documentLibraryService = documentLibraryService;
        _serviceProvider = serviceProvider;

        // Carica i documenti solo dopo che la finestra è stata completamente caricata
        // (necessario per FindResource e per evitare NullReferenceException sui controlli)
        Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        LoadDocuments();
        SearchTextBox.Focus();
    }

    private void LoadDocuments()
    {
        IEnumerable<DocumentInfo> documents;

        if (!string.IsNullOrWhiteSpace(_currentSearchText))
        {
            documents = _documentLibraryService.SearchDocuments(_currentSearchText);
            if (_currentCategory.HasValue)
            {
                documents = documents.Where(d => d.Category == _currentCategory.Value);
            }
        }
        else if (_currentCategory.HasValue)
        {
            documents = _documentLibraryService.GetDocumentsByCategory(_currentCategory.Value);
        }
        else
        {
            documents = _documentLibraryService.GetAllDocuments();
        }

        var documentList = documents.ToList();

        // Aggiorna contatore
        DocumentCountText.Text = documentList.Count == 1
            ? "1 documento"
            : $"{documentList.Count} documenti";

        // Mostra/nascondi pannello "nessun risultato"
        if (documentList.Count == 0)
        {
            DocumentsPanel.Visibility = Visibility.Collapsed;
            NoResultsPanel.Visibility = Visibility.Visible;
            return;
        }

        DocumentsPanel.Visibility = Visibility.Visible;
        NoResultsPanel.Visibility = Visibility.Collapsed;

        // Raggruppa per categoria se non c'è filtro categoria
        DocumentsPanel.Children.Clear();

        if (!_currentCategory.HasValue && string.IsNullOrWhiteSpace(_currentSearchText))
        {
            // Raggruppa per categoria
            var grouped = documentList.GroupBy(d => d.Category);
            foreach (var group in grouped)
            {
                AddCategoryHeader(group.Key);
                foreach (var doc in group)
                {
                    AddDocumentButton(doc);
                }
            }
        }
        else
        {
            // Lista piatta
            foreach (var doc in documentList)
            {
                AddDocumentButton(doc);
            }
        }
    }

    private void AddCategoryHeader(DocumentCategory category)
    {
        var header = new TextBlock
        {
            Text = DocumentLibraryService.GetCategoryDisplayName(category),
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = TryFindBrush("PrimaryBrush", Brushes.DodgerBlue),
            Margin = new Thickness(0, 15, 0, 10)
        };

        // Prima categoria non ha margin top
        if (DocumentsPanel.Children.Count == 0)
        {
            header.Margin = new Thickness(0, 0, 0, 10);
        }

        DocumentsPanel.Children.Add(header);
    }

    private Brush TryFindBrush(string resourceKey, Brush fallback)
    {
        try
        {
            var resource = TryFindResource(resourceKey);
            return resource as Brush ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private Style? TryFindStyle(string resourceKey)
    {
        try
        {
            return TryFindResource(resourceKey) as Style;
        }
        catch
        {
            return null;
        }
    }

    private void AddDocumentButton(DocumentInfo document)
    {
        var button = new Button
        {
            Tag = document
        };

        // Applica lo stile se disponibile
        var style = TryFindStyle("DocumentButtonStyle");
        if (style != null)
        {
            button.Style = style;
        }
        else
        {
            // Fallback style
            button.Padding = new Thickness(15, 12, 15, 12);
            button.Margin = new Thickness(0, 0, 0, 8);
            button.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            button.Cursor = System.Windows.Input.Cursors.Hand;
        }

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Icona
        var iconBorder = new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(8),
            Background = GetCategoryBrush(document.Category),
            VerticalAlignment = VerticalAlignment.Center
        };
        var iconText = new TextBlock
        {
            Text = document.Icon,
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        iconBorder.Child = iconText;
        Grid.SetColumn(iconBorder, 0);
        grid.Children.Add(iconBorder);

        // Testo
        var textPanel = new StackPanel
        {
            Margin = new Thickness(10, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = document.Title,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = TryFindBrush("TextPrimaryBrush", Brushes.Black),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        textPanel.Children.Add(titleText);

        var descText = new TextBlock
        {
            Text = document.Description,
            FontSize = 12,
            Foreground = TryFindBrush("TextSecondaryBrush", Brushes.Gray),
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 3, 0, 0)
        };
        textPanel.Children.Add(descText);

        Grid.SetColumn(textPanel, 1);
        grid.Children.Add(textPanel);

        // Badge tipo documento
        var typeBadge = new Border
        {
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(8, 4, 8, 4),
            VerticalAlignment = VerticalAlignment.Center,
            Background = document.IsPdf
                ? new SolidColorBrush(Color.FromRgb(220, 53, 69))
                : new SolidColorBrush(Color.FromRgb(40, 167, 69))
        };
        var badgeText = new TextBlock
        {
            Text = document.IsPdf ? "PDF" : "HTML",
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White
        };
        typeBadge.Child = badgeText;
        Grid.SetColumn(typeBadge, 2);
        grid.Children.Add(typeBadge);

        button.Content = grid;
        button.Click += DocumentButton_Click;

        DocumentsPanel.Children.Add(button);
    }

    private Brush GetCategoryBrush(DocumentCategory category)
    {
        return category switch
        {
            DocumentCategory.Warfarin => new SolidColorBrush(Color.FromArgb(40, 0, 123, 255)),
            DocumentCategory.DOAC => new SolidColorBrush(Color.FromArgb(40, 111, 66, 193)),
            DocumentCategory.LineGuida => new SolidColorBrush(Color.FromArgb(40, 40, 167, 69)),
            DocumentCategory.Switch => new SolidColorBrush(Color.FromArgb(40, 255, 193, 7)),
            DocumentCategory.PerPazienti => new SolidColorBrush(Color.FromArgb(40, 23, 162, 184)),
            DocumentCategory.Riferimenti => new SolidColorBrush(Color.FromArgb(40, 108, 117, 125)),
            _ => new SolidColorBrush(Color.FromArgb(40, 128, 128, 128))
        };
    }

    private void DocumentButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is DocumentInfo document)
        {
            try
            {
                if (document.IsHtml)
                {
                    // Apri con GuideDialog
                    OpenHtmlGuide(document);
                }
                else
                {
                    // Apri PDF con applicazione predefinita
                    _documentLibraryService.OpenDocument(document);
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(
                    $"Il documento '{document.Title}' non è stato trovato.\n\n" +
                    $"Percorso: {ex.FileName}\n\n" +
                    "Verificare che il file sia presente nella cartella Resources/Guides.",
                    "Documento non trovato",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Errore nell'apertura del documento:\n{ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void OpenHtmlGuide(DocumentInfo document)
    {
        var guideViewModel = _serviceProvider.GetRequiredService<GuideViewModel>();
        guideViewModel.Initialize(document.FileName, document.Title);

        var dialog = new GuideDialog(guideViewModel)
        {
            Owner = this
        };
        dialog.ShowDialog();
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Evita errori durante l'inizializzazione della finestra
        if (!_isLoaded) return;

        var hasText = !string.IsNullOrEmpty(SearchTextBox.Text);
        SearchPlaceholder.Visibility = hasText ? Visibility.Collapsed : Visibility.Visible;
        ClearSearchButton.Visibility = hasText ? Visibility.Visible : Visibility.Collapsed;

        _currentSearchText = SearchTextBox.Text;
        LoadDocuments();
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        SearchTextBox.Text = string.Empty;
        SearchTextBox.Focus();
    }

    private void Category_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton)
        {
            var tag = radioButton.Tag?.ToString();

            if (string.IsNullOrEmpty(tag))
            {
                _currentCategory = null;
            }
            else if (Enum.TryParse<DocumentCategory>(tag, out var category))
            {
                _currentCategory = category;
            }

            // Evita di chiamare LoadDocuments() se la finestra non è ancora stata caricata
            // (l'evento Checked viene chiamato durante InitializeComponent quando IsChecked="True")
            if (_isLoaded)
            {
                LoadDocuments();
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
