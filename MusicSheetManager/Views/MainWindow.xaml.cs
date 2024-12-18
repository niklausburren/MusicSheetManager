using System.Windows;
using Microsoft.Win32;

namespace MusicSheetManager.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ImportDialog ImportDialog { get; }

    public MainWindow(ImportDialog importDialog)
    {
        this.ImportDialog = importDialog;
        
        InitializeComponent();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Select a PDF file"
        };

        if (openFileDialog.ShowDialog(this) == true)
        {
            this.ImportDialog.FileName = openFileDialog.FileName;
            this.ImportDialog.Owner = this;
            this.ImportDialog.ShowDialog();
        }
    }
}