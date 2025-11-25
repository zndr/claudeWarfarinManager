using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WarfarinManager.UI.Models;

namespace WarfarinManager.UI.ViewModels
{
    public partial class INRChartViewModel : ObservableObject
    {
        private readonly List<INRControlDto> _allControls = new();
        private decimal _targetINRMin = 2.0m;
        private decimal _targetINRMax = 3.0m;

        private static readonly SKColor PrimaryBlue = SKColor.Parse("#0078D4");
        private static readonly SKColor SuccessGreen = SKColor.Parse("#107C10");
        private static readonly SKColor ErrorRed = SKColor.Parse("#E81123");
        private static readonly SKColor GridGray = SKColor.Parse("#E0E0E0");
        private static readonly SKColor TextGray = SKColor.Parse("#666666");

        [ObservableProperty] private ObservableCollection<ISeries> _series = new();
        [ObservableProperty] private ObservableCollection<Axis> _xAxes = new();
        [ObservableProperty] private ObservableCollection<Axis> _yAxes = new();
        [ObservableProperty] private decimal _ttrPercentage;
        [ObservableProperty] private string _ttrBackgroundColor = "#666666";
        [ObservableProperty] private string _ttrQualityText = string.Empty;
        [ObservableProperty] private decimal _averageINR;
        [ObservableProperty] private int _controlCount;
        [ObservableProperty] private decimal _inRangePercentage;
        [ObservableProperty] private bool _hasData;
        [ObservableProperty] private bool _hasNoData = true;
        [ObservableProperty] private int _selectedMonths;
        [ObservableProperty] private bool _isThreeMonthsSelected;
        [ObservableProperty] private bool _isSixMonthsSelected = true;
        [ObservableProperty] private bool _isTwelveMonthsSelected;
        [ObservableProperty] private bool _isAllTimeSelected;

        // Punto selezionato
        [ObservableProperty] private bool _hasSelectedPoint;
        [ObservableProperty] private string _selectedDate = string.Empty;
        [ObservableProperty] private string _selectedINR = string.Empty;
        [ObservableProperty] private string _selectedDose = string.Empty;
        [ObservableProperty] private string _selectedPhase = string.Empty;
        [ObservableProperty] private bool _selectedIsInRange;

        public INRChartViewModel()
        {
            SelectedMonths = 6;
            InitializeAxes();
        }

        public void LoadData(IEnumerable<INRControlDto> controls, decimal targetMin, decimal targetMax)
        {
            _allControls.Clear();
            _allControls.AddRange(controls.OrderBy(c => c.ControlDate));
            _targetINRMin = targetMin;
            _targetINRMax = targetMax;
            UpdateChart();
        }

        public void UpdateTTR(decimal ttrValue)
        {
            TtrPercentage = ttrValue;
            if (ttrValue >= 70)
            {
                TtrBackgroundColor = "#107C10";
                TtrQualityText = "(Eccellente)";
            }
            else if (ttrValue >= 60)
            {
                TtrBackgroundColor = "#FFB900";
                TtrQualityText = "(Accettabile)";
            }
            else if (ttrValue >= 50)
            {
                TtrBackgroundColor = "#FF8C00";
                TtrQualityText = "(Subottimale)";
            }
            else
            {
                TtrBackgroundColor = "#E81123";
                TtrQualityText = "(Critico)";
            }
        }

        [RelayCommand]
        private void SetTimeRange(string monthsStr)
        {
            if (int.TryParse(monthsStr, out int months))
            {
                SelectedMonths = months;
                IsThreeMonthsSelected = months == 3;
                IsSixMonthsSelected = months == 6;
                IsTwelveMonthsSelected = months == 12;
                IsAllTimeSelected = months == 0;
                UpdateChart();
            }
        }

        public void OnChartPointClicked(DateTime date)
        {
            var control = _allControls.FirstOrDefault(c => 
                Math.Abs((c.ControlDate - date).TotalDays) < 1);
            
            if (control != null)
            {
                HasSelectedPoint = true;
                SelectedDate = control.ControlDate.ToString("dd/MM/yyyy");
                SelectedINR = control.INRValue.ToString("F2");
                SelectedDose = $"{control.CurrentWeeklyDose:F1} mg/settimana";
                SelectedPhase = control.PhaseDescription;
                SelectedIsInRange = control.IsInRange;
            }
            else
            {
                HasSelectedPoint = false;
            }
        }

        [RelayCommand]
        private void ClearSelection()
        {
            HasSelectedPoint = false;
        }

        private void InitializeAxes()
        {
            XAxes = new ObservableCollection<Axis>
            {
                new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("dd/MM"))
                {
                    LabelsRotation = 0,
                    LabelsPaint = new SolidColorPaint(TextGray),
                    SeparatorsPaint = new SolidColorPaint(GridGray) { StrokeThickness = 1 },
                    TextSize = 11,
                    MinStep = TimeSpan.FromDays(7).Ticks,
                }
            };

            YAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "INR",
                    NamePaint = new SolidColorPaint(TextGray),
                    NameTextSize = 12,
                    Labeler = value => value.ToString("F1"),
                    LabelsPaint = new SolidColorPaint(TextGray),
                    SeparatorsPaint = new SolidColorPaint(GridGray) { StrokeThickness = 1 },
                    TextSize = 11,
                    MinLimit = 0,
                    MaxLimit = 6,
                    MinStep = 0.5
                }
            };
        }

        private void UpdateChart()
        {
            var filteredControls = FilterByTimeRange(_allControls);
            HasData = filteredControls.Any();
            HasNoData = !HasData;

            if (!HasData)
            {
                Series.Clear();
                ControlCount = 0;
                AverageINR = 0;
                InRangePercentage = 0;
                return;
            }

            CalculateStatistics(filteredControls);
            CreateChartSeries(filteredControls);
            UpdateYAxisLimits(filteredControls);
        }

        private List<INRControlDto> FilterByTimeRange(List<INRControlDto> controls)
        {
            if (SelectedMonths <= 0) return controls;
            var cutoffDate = DateTime.Today.AddMonths(-SelectedMonths);
            return controls.Where(c => c.ControlDate >= cutoffDate).ToList();
        }

        private void CalculateStatistics(List<INRControlDto> controls)
        {
            ControlCount = controls.Count;
            AverageINR = controls.Average(c => c.INRValue);
            int inRangeCount = controls.Count(c => c.IsInRange);
            InRangePercentage = ControlCount > 0 ? (decimal)inRangeCount / ControlCount * 100 : 0;
        }

        private void CreateChartSeries(List<INRControlDto> controls)
        {
            Series.Clear();
            if (!controls.Any()) return;

            var inrLineSeries = CreateINRLineSeries(controls);
            Series.Add(inrLineSeries);

            var inRangePoints = CreatePointsSeries(controls.Where(c => c.IsInRange).ToList(), SuccessGreen, "In range");
            if (inRangePoints != null) Series.Add(inRangePoints);

            var outOfRangePoints = CreatePointsSeries(controls.Where(c => !c.IsInRange).ToList(), ErrorRed, "Fuori range");
            if (outOfRangePoints != null) Series.Add(outOfRangePoints);
        }

        private ISeries CreateINRLineSeries(List<INRControlDto> controls)
        {
            var values = controls.Select(c => new DateTimePoint(c.ControlDate, (double)c.INRValue)).ToList();

            return new LineSeries<DateTimePoint>
            {
                Values = values,
                Fill = null,
                Stroke = new SolidColorPaint(PrimaryBlue) { StrokeThickness = 2.5f },
                GeometryFill = new SolidColorPaint(PrimaryBlue),
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                GeometrySize = 8,
                LineSmoothness = 0,
                Name = "Valori INR"
            };
        }

        private ISeries? CreatePointsSeries(List<INRControlDto> controls, SKColor color, string name)
        {
            if (!controls.Any()) return null;
            var values = controls.Select(c => new DateTimePoint(c.ControlDate, (double)c.INRValue)).ToList();

            return new ScatterSeries<DateTimePoint>
            {
                Values = values,
                Fill = new SolidColorPaint(color),
                Stroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                GeometrySize = 10,
                Name = name
            };
        }

        private void UpdateYAxisLimits(List<INRControlDto> controls)
        {
            if (!controls.Any()) return;
            var maxINR = controls.Max(c => c.INRValue);
            var minINR = controls.Min(c => c.INRValue);
            var yMin = Math.Max(0, (double)Math.Min(minINR, _targetINRMin) - 0.5);
            var yMax = Math.Max((double)Math.Max(maxINR, _targetINRMax) + 0.5, 5);
            if (YAxes.Any())
            {
                YAxes[0].MinLimit = yMin;
                YAxes[0].MaxLimit = yMax;
            }
        }
    }
}
