using System.Collections.ObjectModel;
using MusicSheetManager.Models;

namespace MusicSheetManager.Services;

internal class MusicSheetImportService : IMusicSheetImportService
{
    public ObservableCollection<MusicSheet> Import(string fileName)
    {
        var importedMusicSheets = new ObservableCollection<MusicSheet>
        {
            new MusicSheet { Title = "Beispiel 1" },
            new MusicSheet { Title = "Beispiel 2" }
        };
        return importedMusicSheets;
    }
}