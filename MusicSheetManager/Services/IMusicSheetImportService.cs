using System.Collections.ObjectModel;
using MusicSheetManager.Models;

namespace MusicSheetManager.Services;

public interface IMusicSheetImportService
{
    ObservableCollection<MusicSheet> Import(string fileName);
}