using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Autofac;
using AvalonDock.Layout.Serialization;
using Microsoft.Win32;
using MusicSheetManager.Models;
using MusicSheetManager.Properties;
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

        PdfViewer.Source = new Uri("about:blank", UriKind.Absolute);

        this.SourceInitialized += (_, _) => this.RestoreWindowPlacement();
    }

    #endregion


    #region Properties

    public MainWindowViewModel ViewModel => (MainWindowViewModel)this.DataContext;

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

    private static ListViewItem FindListViewItem(DependencyObject source)
    {
        while (source != null && source is not ListViewItem)
        {
            source = VisualTreeHelper.GetParent(source);
        }

        return source as ListViewItem;
    }

    private void RestoreWindowPlacement()
    {
        var s = Settings.Default;

        // Nur anwenden, wenn alle Werte endlich sind
        var invalid =
            double.IsNaN(s.MainWindowLeft) || double.IsInfinity(s.MainWindowLeft) ||
            double.IsNaN(s.MainWindowTop) || double.IsInfinity(s.MainWindowTop) ||
            double.IsNaN(s.MainWindowWidth) || double.IsInfinity(s.MainWindowWidth) ||
            double.IsNaN(s.MainWindowHeight) || double.IsInfinity(s.MainWindowHeight);

        if (invalid)
        {
            return;
        }

        this.WindowStartupLocation = WindowStartupLocation.Manual;

        this.Left = s.MainWindowLeft;
        this.Top = s.MainWindowTop;
        this.Width = Math.Max(200, s.MainWindowWidth);
        this.Height = Math.Max(200, s.MainWindowHeight);

        this.WindowState = s.MainWindowState;
    }

    private void SaveWindowPlacement()
    {
        var s = Settings.Default;

        // Für Maximized/Minimized die RestoreBounds verwenden
        var bounds = this.WindowState == WindowState.Normal ? new Rect(this.Left, this.Top, this.Width, this.Height) : this.RestoreBounds;

        // Nur speichern, wenn die Bounds endlich sind
        if (double.IsFinite(bounds.Left) && double.IsFinite(bounds.Top) &&
            double.IsFinite(bounds.Width) && double.IsFinite(bounds.Height))
        {
            s.MainWindowLeft = bounds.Left;
            s.MainWindowTop = bounds.Top;
            s.MainWindowWidth = bounds.Width;
            s.MainWindowHeight = bounds.Height;
        }

        s.MainWindowState = this.WindowState == WindowState.Minimized ? WindowState.Normal : this.WindowState;

        s.Save();
    }

    private void RestoreDockLayout()
    {
        var layout = Settings.Default.DockLayout;

        if (string.IsNullOrWhiteSpace(layout))
        {
            return;
        }

        var knownContentById = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [MusicSheetsDocument.ContentId] = MusicSheetsDocument.Content,
            [PlaylistsDocument.ContentId] = PlaylistsDocument.Content,
            [PeoplesDocument.ContentId] = PeoplesDocument.Content,
            [PropertiesToolWindow.ContentId] = PropertiesToolWindow.Content,
            [PdfViewerToolWindow.ContentId] = PdfViewerToolWindow.Content
        };

        try
        {
            using var stringReader = new StringReader(layout);
            using var xmlReader = XmlReader.Create(stringReader);

            var serializer = new XmlLayoutSerializer(DockManager);

            serializer.LayoutSerializationCallback += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Model.ContentId) &&
                    knownContentById.TryGetValue(e.Model.ContentId, out var content))
                {
                    e.Content = content;
                    return;
                }

                e.Cancel = true;
            };

            serializer.Deserialize(xmlReader);
        }
        catch
        {
            // Ignore errors
        }
    }

    private void SaveDockLayout()
    {
        try
        {
            var serializer = new XmlLayoutSerializer(DockManager);

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = true
            });

            serializer.Serialize(xmlWriter);
            xmlWriter.Flush();

            Settings.Default.DockLayout = stringWriter.ToString();
            Settings.Default.Save();
        }
        catch
        {
            // Ignore errors
        }
    }

    #endregion


    #region Event Handlers

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            this.RestoreDockLayout();

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

    private void MainWindow_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        this.SaveDockLayout();
        this.SaveWindowPlacement();
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
        PdfViewer.Source = new Uri("about:blank", UriKind.Absolute);

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

    // New: select person on right-click so Delete affects the correct item
    private void PeopleListView_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var listViewItem = FindListViewItem(e.OriginalSource as DependencyObject);

        if (listViewItem != null)
        {
            listViewItem.Focus();
            listViewItem.IsSelected = true;
            e.Handled = true;
        }
    }

    private void PeopleListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        PdfViewer.Source = new Uri("about:blank", UriKind.Absolute);
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
        PdfViewer.Source = new Uri("about:blank", UriKind.Absolute);
        this.ViewModel.SelectedObject = e.NewValue;
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var aboutDialog = new AboutDialog { Owner = this };
        aboutDialog.ShowDialog();
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