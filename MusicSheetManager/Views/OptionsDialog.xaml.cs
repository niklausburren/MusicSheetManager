using System.Windows;
using MusicSheetManager.ViewModels;

namespace MusicSheetManager.Views
{
    public partial class OptionsDialog : Window
    {
        #region Fields

        private readonly OptionsDialogViewModel _viewModel;

        #endregion


        #region Constructors

        public OptionsDialog(OptionsDialogViewModel viewModel)
        {
            this.InitializeComponent();

            _viewModel = viewModel;
            _viewModel.CloseRequested += result =>
            {
                this.DialogResult = result;
                this.Close();
            };

            this.DataContext = _viewModel;
        }

        #endregion
    }
}