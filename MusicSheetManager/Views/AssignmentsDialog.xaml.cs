    using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MusicSheetManager.Models;
using MusicSheetManager.Properties;
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

        public void ShowDialog(Window owner, MusicSheetFolder folder)
        {
            if (this.DataContext is AssignmentsDialogViewModel viewModel)
            {
                viewModel.MusicSheetFolder = folder;
            }

            this.Owner = owner;
            base.ShowDialog();
        }

        #endregion


        #region Event Handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.RestoreExpanderStates();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.SaveExpanderStates();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExpandAllGroups_Click(object sender, RoutedEventArgs e)
        {
            this.SetAllGroupsExpanded(true);
        }

        private void CollapseAllGroups_Click(object sender, RoutedEventArgs e)
        {
            this.SetAllGroupsExpanded(false);
        }

        #endregion


        #region Private Methods

        private void SetAllGroupsExpanded(bool isExpanded)
        {
            var expanders = this.FindVisualChildren<Expander>(this.PeopleListView);

            foreach (var expander in expanders)
            {
                expander.IsExpanded = isExpanded;
            }
        }

        private void SaveExpanderStates()
        {
            var expanders = this.FindVisualChildren<Expander>(this.PeopleListView);
            var collapsedInstruments = new List<string>();

            foreach (var expander in expanders)
            {
                if (!expander.IsExpanded && expander.Tag is string instrumentKey)
                {
                    collapsedInstruments.Add(instrumentKey);
                }
            }

            Settings.Default.CollapsedInstrumentGroups = string.Join(";", collapsedInstruments);
            Settings.Default.Save();
        }

        private void RestoreExpanderStates()
        {
            var collapsedInstruments = Settings.Default.CollapsedInstrumentGroups?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? [];
            var collapsedSet = new HashSet<string>(collapsedInstruments);

            // Give the visual tree time to build
            this.Dispatcher.InvokeAsync(() =>
            {
                var expanders = this.FindVisualChildren<Expander>(this.PeopleListView);

                foreach (var expander in expanders)
                {
                    if (expander.Tag is string instrumentKey)
                    {
                        expander.IsExpanded = !collapsedSet.Contains(instrumentKey);
                    }
                }
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                yield break;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (var childOfChild in this.FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        #endregion
    }
}
