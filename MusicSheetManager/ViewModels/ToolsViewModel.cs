using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MusicSheetManager.Services;

namespace MusicSheetManager.ViewModels;

public class ToolsViewModel
{
    #region Constructors

    public ToolsViewModel(IMusicSheetService musicMusicSheetService, IMusicSheetDistributionService distributionService)
    {
        this.MusicSheetService = musicMusicSheetService;
        this.DistributionService = distributionService;

        this.SplitA3ToA4Command = new RelayCommand(this.SplitPagesFromA3ToA4);
        this.RotatePagesCommand = new RelayCommand(this.RotatePages);
        this.DistributeSheetsCommand = new RelayCommand(this.DistributeSheets);
        this.ExportPartDistributionCommand = new RelayCommand(this.ExportPartDistribution);
    }

    #endregion


    #region Properties

    private IMusicSheetService MusicSheetService { get; }

    private IMusicSheetDistributionService DistributionService { get; }

    public ICommand SplitA3ToA4Command { get; }

    public ICommand RotatePagesCommand { get; }

    public ICommand DistributeSheetsCommand { get; }

    public ICommand ExportPartDistributionCommand { get; }

    #endregion


    #region Private Methods

    private void SplitPagesFromA3ToA4()
    {
        var fileName = ShowOpenFileDialog();

        if (fileName != null)
        {
            try
            {
                this.MusicSheetService.SplitPagesFromA3ToA4(fileName);
                MessageBox.Show("The operation to split A3 pages into A4 pages was successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error splitting A3 pages into A4 pages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RotatePages()
    {
        var fileName = ShowOpenFileDialog();

        if (fileName != null)
        {
            try
            {
                this.MusicSheetService.RotatePages(fileName);
                MessageBox.Show("The operation to rotate pages was successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rotating pages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void DistributeSheets()
    {
        try
        {
            this.DistributionService.Distribute();
            MessageBox.Show("The distribution of the music sheets was successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ExportPartDistribution()
    {
        try
        {
            this.DistributionService.ExportPartDistribution();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting part distribution: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string ShowOpenFileDialog()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Select a PDF file"
        };

        return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
    }

    #endregion
}