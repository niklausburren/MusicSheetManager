using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Autofac;
using Microsoft.Win32;
using MusicSheetManager.Models;
using MusicSheetManager.Utilities;
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


    #region Properties

    private MainWindowViewModel ViewModel
    {
        get { return (MainWindowViewModel)this.DataContext; }
    }

    #endregion


    #region Private Methods

    private static TreeViewItem FindTreeViewItem(DependencyObject source)
    {
        while (source != null && source is not TreeViewItem)
        {
            source = VisualTreeHelper.GetParent(source);
        }

        return source as TreeViewItem;
    }

    #endregion


    #region Event Handlers

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await this.ViewModel.InitializeAsync();

            var cvs = (CollectionViewSource)this.FindResource("GroupedMusicSheetFolders");

            if (cvs?.View is ListCollectionView listView)
            {
                listView.CustomSort = new MusicSheetFolderComparer();
            }

            this.ViewModel.MusicSheetTab.FocusRequested += () =>
            {
                MusicSheetsDocument.IsActive = true;
            };

            this.ViewModel.PlaylistTab.FocusRequested += args => 
            { 
                PlaylistsDocument.IsActive = true;
                PlaylistsTreeView.SetSelectedItem(args.SelectedObject);
                PlaylistsTreeView.ScrollIntoView(args.SelectedObject);
            };

            this.ViewModel.PeopleTab.FocusRequested += args =>
            {
                PeoplesDocument.IsActive = true;
                PeopleListView.SelectedItem = args.SelectedObject;
                PeopleListView.ScrollIntoView(args.SelectedObject);
            };
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

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog { Filter = "PDF files (*.pdf)|*.pdf", Title = "Select a PDF file" };
        
        if (openFileDialog.ShowDialog(this) != true)
        {
            return;
        }
        
        var importDialog = App.Container.Resolve<ImportDialog>();
        importDialog.ShowDialog(this, openFileDialog.FileName);
    }

    private void MusicSheetTreeView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var treeViewItem = FindTreeViewItem(e.OriginalSource as DependencyObject);

        if (treeViewItem != null)
        {
            treeViewItem.Focus();
            treeViewItem.IsSelected = true;
            e.Handled = true;
        }
    }

    private void MusicSheetTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        PdfViewer.Source = new Uri("about:blank");

        switch (e.NewValue)
        {
            case MusicSheetFolder musicSheetFolder:
                this.ViewModel.SelectedObject = musicSheetFolder;
                break;
            case MusicSheet musicSheet:
                PdfViewer.Source = new Uri(musicSheet.FileName, UriKind.Absolute);
                this.ViewModel.SelectedObject = musicSheet;
                break;
            default:
                this.ViewModel.SelectedObject = null;
                break;
        }
    }

    private void PeopleListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PdfViewer.Source = new Uri("about:blank");
        this.ViewModel.SelectedObject = e.AddedItems.OfType<Person>().FirstOrDefault();
    }

    private void PlaylistsTreeView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var treeViewItem = FindTreeViewItem(e.OriginalSource as DependencyObject);
        
        if (treeViewItem != null)
        {
            treeViewItem.Focus();
            treeViewItem.IsSelected = true;
            e.Handled = true;
        }
    }

    private void PlaylistsTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        this.ViewModel.SelectedObject = e.NewValue;
    }

    #endregion


    #region Class MusicSheetFolderComparer

    private sealed class MusicSheetFolderComparer : IComparer
    {
        #region IComparer Members

        public int Compare(object x, object y)
        {
            var titleX = (x as MusicSheetFolder)?.Title;
            var titleY = (y as MusicSheetFolder)?.Title;

            var sortKeyX = Converters.FirstLetterConverter.GetSortKey(titleX);
            var sortKeyY = Converters.FirstLetterConverter.GetSortKey(titleY);

            return string.Compare(sortKeyX, sortKeyY, StringComparison.Ordinal);
        }

        #endregion
    }

    #endregion
}