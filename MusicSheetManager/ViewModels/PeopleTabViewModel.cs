using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.Models;
using MusicSheetManager.Services;

namespace MusicSheetManager.ViewModels;

public class PeopleTabViewModel : ObservableObject
{
    #region Constructors

    public PeopleTabViewModel(IPeopleService peopleService)
    {
        this.PeopleService = peopleService;
    }

    #endregion


    #region Properties

    private IPeopleService PeopleService { get; }

    public ObservableCollection<Person> People => this.PeopleService.People;

    #endregion


    #region Public Methods

    public async Task InitializeAsync()
    {
        await this.PeopleService.LoadAsync();
    }

    #endregion
}