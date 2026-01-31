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

        foreach (var part in PartInfo.All.OrderBy(i => i.DisplayName))
        {
            items.Add(part);
        }

        return items;
    }

    #endregion
}