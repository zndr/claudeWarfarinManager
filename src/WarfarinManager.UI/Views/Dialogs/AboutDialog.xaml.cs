using System.Reflection;
using System.Windows;

namespace WarfarinManager.UI.Views.Dialogs;

public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
        SetVersionText();
    }

    private void SetVersionText()
    {
        // Usa GetEntryAssembly per consistenza con UpdateNotificationService
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var versionString = version != null
            ? $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}"
            : "1.0.0.0";
        VersionText.Text = $"Versione {versionString}";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
