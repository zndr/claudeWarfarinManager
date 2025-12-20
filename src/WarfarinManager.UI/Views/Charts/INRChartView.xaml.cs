using System.Windows.Controls;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Kernel;
using LiveChartsCore.Defaults;
using WarfarinManager.UI.ViewModels;
using System.Linq;

namespace WarfarinManager.UI.Views.Charts
{
    /// <summary>
    /// Code-behind per INRChartView
    /// </summary>
    public partial class INRChartView : UserControl
    {
        public INRChartView()
        {
            InitializeComponent();
        }

        private void ChartControl_ChartPointPointerDown(IChartView chart, ChartPoint? point)
        {
            if (point != null && DataContext is INRChartViewModel viewModel)
            {
                // Estrai il valore secondario (data in Ticks) e primario (INR)
                var dateTicks = (long)point.SecondaryValue;
                var dateTime = new System.DateTime(dateTicks);
                
                viewModel.OnChartPointClicked(dateTime);
            }
        }
    }
}
