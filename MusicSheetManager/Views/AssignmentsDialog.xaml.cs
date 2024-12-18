using System.Windows;
using MusicSheetManager.Models;
using MusicSheetManager.ViewModels;

namespace MusicSheetManager.Views
{
    /// <summary>
    /// Interaktionslogik für AssignmentsDialog.xaml
    /// </summary>
    public partial class AssignmentsDialog : Window
    {
        #region Constructors

        public AssignmentsDialog(AssignmentsDialogViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
            viewModel.SetDialogResultAction = result => this.DialogResult = result;
        }

        #endregion


        #region Public Methods

        public void ShowDialog(Window owner, MusicSheetFolder musicSheetFolder)
        {
            if (this.DataContext is AssignmentsDialogViewModel viewModel)
            {
                viewModel.MusicSheetFolder = musicSheetFolder;
            }

            this.Owner = owner;
            base.ShowDialog();
        }

        #endregion


        #region Event Handlers

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
