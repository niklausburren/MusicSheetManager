using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autofac;
using Microsoft.Win32;
using MusicSheetManager.Models;
using MusicSheetManager.ViewModels;

namespace MusicSheetManager.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Constructors

    public MainWindow(MainWindowViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
    }

    #endregion


    #region Event Handlers

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await ((MainWindowViewModel)this.DataContext).InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "MusicSheetManager", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is TreeViewItem item && item.DataContext is MusicSheetFolder folder)
        {
            e.Handled = true;
            var assignmentsDialog = App.Container.Resolve<AssignmentsDialog>();
            assignmentsDialog.ShowDialog(this, folder);
        }
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Select a PDF file"
        };

        if (openFileDialog.ShowDialog(this) != true)
        {
            return;
        }

        var importDialog = App.Container.Resolve<ImportDialog>();
        importDialog.ShowDialog(this, openFileDialog.FileName);
    }

    #endregion
}