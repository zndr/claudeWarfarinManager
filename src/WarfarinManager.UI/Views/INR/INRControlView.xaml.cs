using System.Windows;
using WarfarinManager.UI.ViewModels;

namespace WarfarinManager.UI.Views.INR
{
    /// <summary>
    /// Interaction logic for INRControlView.xaml
    /// </summary>
    public partial class INRControlView : Window
    {
        public INRControlView()
        {
            InitializeComponent();

            // Adatta la finestra alle dimensioni dello schermo all'avvio
            Loaded += INRControlView_Loaded;
        }

        /// <summary>
        /// Adatta le dimensioni della finestra allo schermo disponibile.
        /// Su schermi piccoli (es. 15 pollici), ridimensiona la finestra per non uscire dai bordi.
        /// </summary>
        private void INRControlView_Loaded(object sender, RoutedEventArgs e)
        {
            // Ottieni le dimensioni dell'area di lavoro (esclude la taskbar)
            var workArea = SystemParameters.WorkArea;

            // Se la finestra è più grande dello schermo, adatta le dimensioni
            if (Width > workArea.Width)
            {
                Width = workArea.Width * 0.95; // 95% della larghezza disponibile
            }

            if (Height > workArea.Height)
            {
                Height = workArea.Height * 0.95; // 95% dell'altezza disponibile
            }

            // Centra la finestra sullo schermo
            Left = (workArea.Width - Width) / 2 + workArea.Left;
            Top = (workArea.Height - Height) / 2 + workArea.Top;

            // Assicura che la finestra sia completamente visibile
            if (Left < workArea.Left)
                Left = workArea.Left;
            if (Top < workArea.Top)
                Top = workArea.Top;
            if (Left + Width > workArea.Right)
                Left = workArea.Right - Width;
            if (Top + Height > workArea.Bottom)
                Top = workArea.Bottom - Height;
        }
    }
}
