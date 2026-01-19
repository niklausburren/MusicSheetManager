using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace MusicSheetManager.Utilities
{
    public static class TreeViewExtensions
    {
        #region Public Methods

        public static void SetSelectedItem(this TreeView treeView, object item)
        {
            if (treeView == null || item == null)
            {
                return;
            }

            if (treeView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                treeView.Dispatcher.BeginInvoke((Action)Select, DispatcherPriority.Loaded);
            }
            else
            {
                EventHandler handler = null;
                handler = (_, _) =>
                {
                    if (treeView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        return;
                    }

                    treeView.ItemContainerGenerator.StatusChanged -= handler;
                    treeView.Dispatcher.BeginInvoke((Action)Select, DispatcherPriority.Loaded);
                };
                treeView.ItemContainerGenerator.StatusChanged += handler;
            }

            return;

            void Select()
            {
                treeView.UpdateLayout();
                var container = FindContainer(treeView, item);

                if (container != null)
                {
                    container.IsSelected = true;
                    container.Focus();
                }
            }
        }

        public static void ScrollIntoView(this TreeView treeView, object item)
        {
            if (treeView == null || item == null)
            {
                return;
            }

            if (treeView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                treeView.Dispatcher.BeginInvoke((Action)DoScroll, DispatcherPriority.Loaded);
            }
            else
            {
                EventHandler handler = null;
                handler = (_, _) =>
                {
                    if (treeView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                    {
                        return;
                    }

                    treeView.ItemContainerGenerator.StatusChanged -= handler;
                    treeView.Dispatcher.BeginInvoke((Action)DoScroll, DispatcherPriority.Loaded);
                };
                treeView.ItemContainerGenerator.StatusChanged += handler;
            }

            return;

            void DoScroll()
            {
                treeView.UpdateLayout();
                var container = FindContainer(treeView, item);
                if (container != null)
                {
                    container.BringIntoView();
                }
            }
        }

        #endregion


        #region Private Methods

        private static TreeViewItem FindContainer(ItemsControl parent, object item)
        {
            if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem direct)
            {
                return direct;
            }

            foreach (var child in parent.Items)
            {
                if (parent.ItemContainerGenerator.ContainerFromItem(child) is not TreeViewItem parentContainer)
                {
                    continue;
                }

                var wasExpanded = parentContainer.IsExpanded;
                parentContainer.IsExpanded = true;
                parentContainer.UpdateLayout();

                var match = FindContainer(parentContainer, item);
                if (match != null)
                {
                    return match;
                }

                parentContainer.IsExpanded = wasExpanded;
            }

            return null;
        }

        #endregion
    }
}