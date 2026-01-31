using System.Windows;

namespace MusicSheetManager.Views;

public partial class SplashScreen : Window
{
    #region Fields

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(
            nameof(Progress),
            typeof(int),
            typeof(SplashScreen),
            new PropertyMetadata(0));

    #endregion


    #region Constructors

    public SplashScreen()
    {
        this.InitializeComponent();
    }

    #endregion


    #region Properties

    public int Progress
    {
        get => (int)this.GetValue(ProgressProperty);
        set => this.SetValue(ProgressProperty, value);
    }

    #endregion
}