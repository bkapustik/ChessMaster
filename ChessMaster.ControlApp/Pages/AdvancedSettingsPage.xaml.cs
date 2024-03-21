using ChessMaster.ControlApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;

namespace ChessMaster.ControlApp.Pages;

public sealed partial class AdvancedSettingsPage : Page, INotifyPropertyChanged
{
    private const string DISABLE_OTZU = "Disable otsu";
    private const string ENABLE_OTZU = "Enable otsu";
    private const string SET_DEFAULT_METRIC = "Set default metric";
    private const string SET_QUADRATIC_METRIC = "Set quadratic metric";

    private IUIKinectService KinectService;

    private string milimetersClippedFromFigure;
    public string MilimetersClippedFromFigure
    {
        get { return milimetersClippedFromFigure; }
        set
        {
            milimetersClippedFromFigure = value;
            OnPropertyChanged(nameof(MilimetersClippedFromFigure));
        }
    }

    private string pointsIndicatingFigure;
    public string PointsIndicatingFigure
    {
        get { return pointsIndicatingFigure; }
        set
        {
            pointsIndicatingFigure = value;
            OnPropertyChanged(nameof(PointsIndicatingFigure));
        }
    }

    private string milisecondsTasks;
    public string MilisecondsTasks
    {
        get { return milisecondsTasks; }
        set
        {
            milisecondsTasks = value;
            OnPropertyChanged(nameof(MilisecondsTasks));
        }
    }

    private string binarizationThreshold;
    public string BinarizationThreshold
    {
        get { return binarizationThreshold; }
        set
        {
            binarizationThreshold = value;
            OnPropertyChanged(nameof(BinarizationThreshold));
        }
    }

    private string infraredFilterThreshold;
    public string InfraredFilterThreshold
    {
        get { return infraredFilterThreshold; }
        set
        {
            infraredFilterThreshold = value;
            OnPropertyChanged(nameof(InfraredFilterThreshold));
        }
    }

    private string disruptionDetectionThreshold;
    public string DisruptionDetectionThreshold
    {
        get { return disruptionDetectionThreshold; }
        set
        {
            disruptionDetectionThreshold = value;
            OnPropertyChanged(nameof(DisruptionDetectionThreshold));
        }
    }

    private string distanceMetricFittingChessboard;
    public string DistanceMetricFittingChessboard
    {
        get { return distanceMetricFittingChessboard; }
        set
        {
            distanceMetricFittingChessboard = value;
            OnPropertyChanged(nameof(DistanceMetricFittingChessboard));
        }
    }

    private bool Initialized { get; set; }

    public AdvancedSettingsPage()
    {
        this.InitializeComponent();
        this.DataContext = this;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        KinectService = App.Services.GetRequiredService<IUIKinectService>();
        PrepareComponentsValues();
    }

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private void PrepareComponentsValues()
    {
        var parameters = KinectService.UserDefinedParameters.GetShallowCopy();

        MilimetersClippedFromFigure = parameters.MilimetersClippedFromFigure.ToString();
        MilimetersClippedTrackBar.Value = parameters.MilimetersClippedFromFigure;

        PointsIndicatingFigure = parameters.NumberOfPointsIndicatingFigure.ToString();
        PointsIndicatingFigureTrackBar.Value = parameters.NumberOfPointsIndicatingFigure;

        milisecondsTasks = parameters.MinimalTimeBetweenTrackingTasksInMiliseconds.ToString();
        MilisecondsTasksTrackBar.Value = parameters.MinimalTimeBetweenTrackingTasksInMiliseconds;

        BinarizationThreshold = parameters.BinarizationThreshold.ToString();
        BinarizationThresholdTrackbar.Value = parameters.BinarizationThreshold;

        OtzuToggleButton.Content = parameters.OtzuActiveInBinarization ? DISABLE_OTZU : ENABLE_OTZU;

        FiguresColorMetricButton.Content = parameters.IsFiguresColorMetricExperimental
            ? SET_DEFAULT_METRIC
            : SET_QUADRATIC_METRIC;

        DistanceMetricFittingChessboardButton.Content = parameters.IsDistanceMetricInChessboardFittingExperimental
            ? SET_DEFAULT_METRIC
            : SET_QUADRATIC_METRIC;
        DistanceMetricFittingChessboardTrackBar.Value = parameters.ClippedDistanecInChessboardFittingMetric;
        distanceMetricFittingChessboard = parameters.ClippedDistanecInChessboardFittingMetric.ToString();

        InfluenceColorTrackbar.Value = parameters.GameStateInfluenceOnColor;

        InfluencePresenceTrackBar.Value = parameters.GameStateInfluenceOnPresence;
        InfluencePresenceTrackBar.Maximum = parameters.NumberOfPointsIndicatingFigure - 1;

        InfraredFilterThresholdTtrackBar.Value = parameters.InfraredPointFilterThreshold;
        InfraredFilterThreshold = parameters.InfraredPointFilterThreshold.ToString();

        DisruptionDetectionThresholdTrackBar.Value = parameters.DisruptionDetectionThreshold;
        DisruptionDetectionThreshold = parameters.DisruptionDetectionThreshold.ToString();

        Initialized = true;
    }


    private void MilimetersClippedTrackBar_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        MilimetersClippedFromFigure = MilimetersClippedTrackBar.Value.ToString();
        KinectService.UserDefinedParameters.ChangePrototype(x => x.MilimetersClippedFromFigure = (int)MilimetersClippedTrackBar.Value);
    }
    private void PointsIndicatingFigureTrackBar_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        PointsIndicatingFigure = PointsIndicatingFigureTrackBar.Value.ToString();
        KinectService.UserDefinedParameters.ChangePrototype(x => x.NumberOfPointsIndicatingFigure = (int)PointsIndicatingFigureTrackBar.Value);

        if (InfluencePresenceTrackBar?.Value >= PointsIndicatingFigureTrackBar.Value)
            InfluencePresenceTrackBar.Value = PointsIndicatingFigureTrackBar.Value - 1;

        InfluencePresenceTrackBar.Maximum = PointsIndicatingFigureTrackBar.Value - 1;
        KinectService.UserDefinedParameters.ChangePrototype(x => x.GameStateInfluenceOnPresence = (int)InfluencePresenceTrackBar.Value);
    }
    private void MilisecondsTasksTrackBar_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        MilisecondsTasks = MilisecondsTasksTrackBar.Value.ToString();
        KinectService.UserDefinedParameters.ChangePrototype(x => x.MinimalTimeBetweenTrackingTasksInMiliseconds = (int)MilisecondsTasksTrackBar.Value);
    }
    private void BinarizationThresholdTrackbar_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        BinarizationThreshold = BinarizationThresholdTrackbar.Value.ToString();
        KinectService.UserDefinedParameters.ChangePrototype(x => x.BinarizationThreshold = (int)BinarizationThresholdTrackbar.Value);
    }
    private void OtzuToggleButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        if ((string)OtzuToggleButton.Content == DISABLE_OTZU)
        {
            OtzuToggleButton.Content = ENABLE_OTZU;
            KinectService.UserDefinedParameters.ChangePrototype(x => x.OtzuActiveInBinarization = false);
        }
        else
        {
            OtzuToggleButton.Content = DISABLE_OTZU;
            KinectService.UserDefinedParameters.ChangePrototype(x => x.OtzuActiveInBinarization = true);
        }
    }
    private void InfraredFilterThresholdTtrackBar_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        InfraredFilterThreshold = InfraredFilterThresholdTtrackBar.Value.ToString();
        KinectService.UserDefinedParameters.ChangePrototype(x => x.InfraredPointFilterThreshold = (int)InfraredFilterThresholdTtrackBar.Value);
    }
    private void DisruptionDetectionThresholdTrackBar_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        DisruptionDetectionThreshold = DisruptionDetectionThresholdTrackBar.Value.ToString();
        KinectService.UserDefinedParameters.ChangePrototype(x => x.DisruptionDetectionThreshold = (int)DisruptionDetectionThresholdTrackBar.Value);
    }
    private void FiguresColorMetricButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        if ((string)FiguresColorMetricButton.Content == SET_DEFAULT_METRIC)
        {
            FiguresColorMetricButton.Content = SET_QUADRATIC_METRIC;
            KinectService.UserDefinedParameters.ChangePrototype(x => x.IsFiguresColorMetricExperimental = false);
        }
        else
        {
            FiguresColorMetricButton.Content = SET_DEFAULT_METRIC;
            KinectService.UserDefinedParameters.ChangePrototype(x => x.IsFiguresColorMetricExperimental = true);
        }
    }
    private void DistanceMetricFittingChessboardTrackBar_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        DistanceMetricFittingChessboard = DistanceMetricFittingChessboardTrackBar.Value.ToString();
        KinectService.UserDefinedParameters.ChangePrototype(x => x.ClippedDistanecInChessboardFittingMetric = (int)DistanceMetricFittingChessboardTrackBar.Value);
    }
    private void DistanceMetricFittingChessboardButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        if ((string)DistanceMetricFittingChessboardButton.Content == SET_DEFAULT_METRIC)
        {
            DistanceMetricFittingChessboardButton.Content = SET_QUADRATIC_METRIC;
            KinectService.UserDefinedParameters.ChangePrototype(x => x.IsDistanceMetricInChessboardFittingExperimental = false);
        }
        else
        {
            DistanceMetricFittingChessboardButton.Content = SET_DEFAULT_METRIC;
            KinectService.UserDefinedParameters.ChangePrototype(x => x.IsDistanceMetricInChessboardFittingExperimental = true);
        }
    }
    private void InfluenceColorTrackbar_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        KinectService.UserDefinedParameters.ChangePrototype(x => x.GameStateInfluenceOnPresence = (int)InfluencePresenceTrackBar.Value);
    }
    private void InfluencePresenceTrackBar_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!Initialized)
        {
            return;
        }

        KinectService.UserDefinedParameters.ChangePrototype(x => x.GameStateInfluenceOnPresence = (int)InfluencePresenceTrackBar.Value);
    }
}
