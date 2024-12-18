using MusicSheetManager.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace MusicSheetManager.Views
{
    /// <summary>
    /// Interaktionslogik für ImportDialog.xaml
    /// </summary>
    public partial class ImportDialog : Window
    {
        public string FileName { get; set; }

        public ImportDialog(ImportViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
