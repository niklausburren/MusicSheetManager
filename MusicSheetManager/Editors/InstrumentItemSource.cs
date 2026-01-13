using System.Linq;
using MusicSheetManager.Models;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MusicSheetManager.Editors;

public sealed class InstrumentItemsSource : IItemsSource
{
    #region IItemsSource Members

    public ItemCollection GetValues()
    {
        var items = new ItemCollection();

        var instruments = InstrumentInfo.All
            .Where(i => i != InstrumentInfo.Unknown)
            .OrderBy(i => i.DisplayName);

        foreach (var instrument in instruments)
        {
            items.Add(instrument);
        }

        return items;
    }

    #endregion
}