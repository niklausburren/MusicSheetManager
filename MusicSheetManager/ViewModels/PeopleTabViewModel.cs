using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.Models;
using MusicSheetManager.Services;

namespace MusicSheetManager.ViewModels;

public class PeopleTabViewModel : ObservableObject
{
    #region Fields

    private bool _isFocused;

    #endregion


    #region Constructors

    public PeopleTabViewModel(IPeopleService peopleService)
    {
        this.PeopleService = peopleService;
    }

    #endregion


    #region Properties

    private IPeopleService PeopleService { get; }

    public ObservableCollection<Person> People => this.PeopleService.People;

    public bool IsFocused
    {
        get => _isFocused;
        set => this.SetProperty(ref _isFocused, value);
    }

    #endregion


    #region Public Methods

    public async Task InitializeAsync()
    {
        await this.PeopleService.LoadAsync();
    }

    public void DeletePerson(Person person)
    {
        this.PeopleService.People.Remove(person);
        this.PeopleService.SaveAsync();
    }

    #endregion
}