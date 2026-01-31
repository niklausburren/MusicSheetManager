using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicSheetManager.Models;
using MusicSheetManager.Services;

namespace MusicSheetManager.ViewModels;

public class PeopleTabViewModel : ObservableObject
{
    #region Events

    public event Action<FocusRequestedEventArgs> FocusRequested;

    #endregion


    #region Constructors

    public PeopleTabViewModel(IPeopleService peopleService)
    {
        this.PeopleService = peopleService;

        this.CreatePersonCommand = new AsyncRelayCommand(this.CreatePersonAsync);
    }

    #endregion


    #region Properties

    private IPeopleService PeopleService { get; }

    public ObservableCollection<Person> People => this.PeopleService.People;

    public ICommand CreatePersonCommand { get; }

    #endregion


    #region Public Methods

    public async Task InitializeAsync()
    {
        await this.PeopleService.LoadAsync();
    }

    public async Task DeletePersonAsync(Person person)
    {
        this.FocusRequested?.Invoke(FocusRequestedEventArgs.Empty);

        var result = MessageBox.Show(
            Application.Current.MainWindow!,
            $"Do you really want to delete person \"{person.FullName}\"?",
            "Delete Person",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.No)
        {
            return;
        }

        this.PeopleService.People.Remove(person);
        await this.PeopleService.SaveAsync();
    }

    #endregion


    #region Private Methods

    private async Task CreatePersonAsync()
    {
        var person = new Person(
            Guid.NewGuid(),
            firstName: "Person",
            lastName: "New",
            instrument: InstrumentInfo.TrumpetBb,
            part: PartInfo.First,
            clef: ClefInfo.TrebleClef);

        this.PeopleService.People.Add(person);
        await this.PeopleService.SaveAsync();

        this.FocusRequested?.Invoke(new FocusRequestedEventArgs(person));
    }

    #endregion
}

public class FocusRequestedEventArgs
{
    #region Constructors

    public FocusRequestedEventArgs(object selectedObject)
    {
        this.SelectedObject = selectedObject;
    }

    #endregion


    #region Properties

    public static FocusRequestedEventArgs Empty { get; } = new FocusRequestedEventArgs(null!);

    public object SelectedObject { get; }

    #endregion
}