using System;
using System.Diagnostics;
using System.Windows;
using WarfarinManager.Core.Models;

namespace WarfarinManager.UI.Views.Dialogs;

/// <summary>
/// Finestra di notifica per nuovi aggiornamenti
/// </summary>
public partial class UpdateNotificationWindow : Window
{
    private readonly UpdateInfo _updateInfo;
    private readonly string _currentVersion;

    public UpdateNotificationWindow(UpdateInfo updateInfo, string currentVersion)
    {
        InitializeComponent();

        _updateInfo = updateInfo ?? throw new ArgumentNullException(nameof(updateInfo));
        _currentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));

        LoadUpdateInfo();
    }

    private void LoadUpdateInfo()
    {
        // Versione
        VersionTextBlock.Text = $"Versione {_updateInfo.Version} disponibile (attuale: {_currentVersion})";
        NewVersionTextBlock.Text = _updateInfo.Version;

        // Data di rilascio
        ReleaseDateTextBlock.Text = _updateInfo.ReleaseDate.ToString("dd/MM/yyyy");

        // Dimensione file
        FileSizeTextBlock.Text = FormatFileSize(_updateInfo.FileSize);

        // Hash SHA256 (se presente)
        if (!string.IsNullOrWhiteSpace(_updateInfo.Sha256Hash))
        {
            HashLabelTextBlock.Visibility = Visibility.Visible;
            HashTextBlock.Visibility = Visibility.Visible;
            HashTextBlock.Text = _updateInfo.Sha256Hash.ToLowerInvariant();
        }

        // Note di rilascio
        ReleaseNotesTextBox.Text = _updateInfo.ReleaseNotes;

        // Badge critico
        if (_updateInfo.IsCritical)
        {
            CriticalBadge.Visibility = Visibility.Visible;
        }

        // Titolo finestra con versione
        Title = $"Aggiornamento Disponibile - v{_updateInfo.Version}";
    }

    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Apri il browser con l'URL di download
            var psi = new ProcessStartInfo
            {
                FileName = _updateInfo.DownloadUrl,
                UseShellExecute = true
            };
            Process.Start(psi);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Impossibile aprire il link di download:\n{ex.Message}\n\nURL: {_updateInfo.DownloadUrl}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void RemindLaterButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes == 0)
            return "Sconosciuta";

        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}
