using System.Collections.ObjectModel;
using System.Linq;
using MusicSheetManager.Models;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace MusicSheetManager.Editors;

public sealed class PartsEditor : ITypeEditor
{
    public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
    {
        var musicSheet = (MusicSheet)propertyItem.Instance;

        var editor = new CheckComboBox
        {
            ItemsSource = PartInfo.All,
            DisplayMemberPath = "DisplayName",
            Delimiter = ", ",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 22,
            Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops =
                [
                    new GradientStop((Color)ColorConverter.ConvertFromString("#efefef")!, 0.0),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#e5e5e5")!, 1.0)
                ]
            }
        };

        editor.Loaded += (_, _) =>
        {
            editor.SelectedItemsOverride = musicSheet.Parts;

            editor.ItemSelectionChanged += (_, _) =>
            {
                propertyItem.Value = new ObservableCollection<PartInfo>(editor.SelectedItems.OfType<PartInfo>());
            };
        };

        return editor;
    }
}