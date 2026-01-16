using System.Linq;
using MusicSheetManager.Models;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MusicSheetManager.Editors;

public sealed class PartItemsSource : IItemsSource
{
    #region IItemsSource Members

    public ItemCollection GetValues()
    {
        var items = new ItemCollection();

        var parts = PartInfo.All
            .OrderBy(i => i.DisplayName);

        foreach (var part in parts)
        {
            items.Add(part);
        }

        return items;
    }

    #endregion
}