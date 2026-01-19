using System;
using System.Windows;
using System.Windows.Controls;
using MusicSheetManager.Models;
using MusicSheetManager.ViewModels;

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

        public void ShowDialog(Window owner, string fileName, MusicSheetFolder musicSheetFolder = null)
        {
            if (this.DataContext is ImportDialogViewModel viewModel)
            {
                viewModel.FileName = fileName;

                if (musicSheetFolder != null)
                {
                    viewModel.Metadata.Title = musicSheetFolder.Title;
                    viewModel.Metadata.Composer = musicSheetFolder.Composer;
                    viewModel.Metadata.Arranger = musicSheetFolder.Arranger;
                }
            }

            this.Owner = owner;
            PdfViewer.Source = new Uri(fileName);
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
