using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MusicSheetManager.Models;

namespace MusicSheetManager.Services
{
    public interface IPeopleService
    {
        #region Properties

        ObservableCollection<Person> People { get; }

        #endregion


        #region Public Methods

        Task LoadAsync();

        Task SaveAsync();

        #endregion
    }
}
