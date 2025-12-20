using System.Windows.Controls;
using WarfarinManager.UI.ViewModels;
using Serilog;

namespace WarfarinManager.UI.Views.Patient;

/// <summary>
/// Interaction logic for PatientDetailsView.xaml
/// </summary>
public partial class PatientDetailsView : UserControl
{
    public PatientDetailsView(PatientDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        // Find the TabControl and count tabs
        var tabControl = FindTabControl(this);
        if (tabControl != null)
        {
            Log.Information("=== PATIENT DETAILS VIEW LOADED ===");
            Log.Information($"TabControl found with {tabControl.Items.Count} tabs");

            for (int i = 0; i < tabControl.Items.Count; i++)
            {
                if (tabControl.Items[i] is TabItem tabItem)
                {
                    Log.Information($"  Tab {i}: {tabItem.Header}");
                }
            }
        }
        else
        {
            Log.Warning("TabControl not found in PatientDetailsView!");
        }
    }

    private TabControl? FindTabControl(System.Windows.DependencyObject parent)
    {
        if (parent is TabControl tc)
            return tc;

        int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            var result = FindTabControl(child);
            if (result != null)
                return result;
        }

        return null;
    }
}
