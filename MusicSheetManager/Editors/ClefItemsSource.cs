using MusicSheetManager.Models;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MusicSheetManager.Editors;

public sealed class ClefItemsSource : IItemsSource
{
    #region IItemsSource Members

    public ItemCollection GetValues()
    {
        var items = new ItemCollection();

        foreach (var clef in ClefInfo.All)
        {
            items.Add(clef);
        }

        return items;
    }

    #endregion
}