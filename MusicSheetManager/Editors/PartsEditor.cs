using System.Linq;
using System.Windows;
using System.Windows.Media;
using MusicSheetManager.Models;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace MusicSheetManager.Editors;

public sealed class PartsEditor : ITypeEditor
{
    #region ITypeEditor Members

    public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
    {
        var musicSheet = (MusicSheet)propertyItem.Instance;

        var editor = new CheckComboBox
        {
            ItemsSource = PartInfo.All.OrderBy(p => p.Index).Where(p => p != PartInfo.None),
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
        };

        return editor;
    }

    #endregion
}