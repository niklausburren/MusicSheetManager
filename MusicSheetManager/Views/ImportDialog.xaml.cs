using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MusicSheetManager.Models;
using MusicSheetManager.ViewModels;
using OfficeOpenXml.Drawing.Slicer.Style;

namespace MusicSheetManager.Views
{
    public partial class ImportDialog : Window
    {
        #region Constructors

        public ImportDialog(ImportDialogViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
            viewModel.SetDialogResultAction = result => this.DialogResult = result;
        }

        #endregion


        #region Public Methods

        public void ShowDialog(Window owner, IEnumerable<string> fileNames, MusicSheetFolder musicSheetFolder = null)
        {
            if (this.DataContext is not ImportDialogViewModel viewModel)
            {
                return;
            }

            viewModel.FileNames = fileNames.ToList();

            if (musicSheetFolder != null)
            {
                viewModel.Metadata.Title = musicSheetFolder.Title;
                viewModel.Metadata.Composer = musicSheetFolder.Composer;
                viewModel.Metadata.Arranger = musicSheetFolder.Arranger;
                viewModel.IsMetadataEditable = false;
            }
            else
            {
                viewModel.IsMetadataEditable = true;
            }

            PdfViewer.Source = new Uri(viewModel.FileNames[0]);

            this.Owner = owner;
            base.ShowDialog();
        }

        #endregion


        #region Event Handlers

        private void MusicSheetListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MusicSheetListView.SelectedItem is MusicSheetInfo musicSheet)
            {
                PdfViewer.Source = new Uri(musicSheet.FileName);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
