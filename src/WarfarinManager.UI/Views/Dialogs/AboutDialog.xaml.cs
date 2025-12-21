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
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var versionString = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.1.2";
        VersionText.Text = $"Versione {versionString}";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
