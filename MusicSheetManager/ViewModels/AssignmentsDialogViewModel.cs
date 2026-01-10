using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicSheetManager.Models;
using MusicSheetManager.Services;

namespace MusicSheetManager.ViewModels;

public class AssignmentsDialogViewModel : ObservableObject
{
    #region Fields

    private ObservableCollection<PersonInfo> _people;

    #endregion


    #region Constructors

    public AssignmentsDialogViewModel(IPeopleService peopleService, IMusicSheetAssignmentService musicSheetAssignmentService)
    {
        this.PeopleService = peopleService;
        this.MusicSheetAssignmentService = musicSheetAssignmentService;
        this.UpdateAssignments = new RelayCommand(this.OnUpdateAssignments);
    }

    #endregion


    #region Properties

    public ICommand UpdateAssignments { get; }

    private IPeopleService PeopleService { get; }

    private IMusicSheetAssignmentService MusicSheetAssignmentService { get; }

    public ObservableCollection<PersonInfo> People
    {
        get
        {
            if (_people == null)
            {
                _people = new ObservableCollection<PersonInfo>();

                foreach (var person in this.PeopleService.People.Where(p => p.Instrument.Category != InstrumentCategory.Percussion))
                {
                    _people.Add(new PersonInfo(
                        person,
                        this.MusicSheetAssignmentService.GetAssignableMusicSheets(this.MusicSheetFolder, person),
                        this.MusicSheetAssignmentService.GetAssignedMusicSheet(this.MusicSheetFolder, person)));
                }
            }

            return _people;
        }
    }

    public MusicSheetFolder MusicSheetFolder { get; set; }

    public Action<bool?> SetDialogResultAction { get; set; }

    #endregion


    #region Private Methods

    private void OnUpdateAssignments()
    {
        foreach (var assignment in this.MusicSheetAssignmentService.Assignments.Where(a => a.MusicSheetFolderId == this.MusicSheetFolder.Id).ToList())
        {
            this.MusicSheetAssignmentService.Assignments.Remove(assignment);
        }

        foreach (var personInfo in this.People)
        {
            if (personInfo.SelectedMusicSheet != null && 
                personInfo.SelectedMusicSheet != this.MusicSheetAssignmentService.GetDefaultMusicSheet(this.MusicSheetFolder, personInfo.Person))
            {
                var assignment = new MusicSheetAssignment(
                    this.MusicSheetFolder.Id,
                    personInfo.SelectedMusicSheet?.Id ?? Guid.Empty,
                    personInfo.Person.Id);

                this.MusicSheetAssignmentService.Assignments.Add(assignment);
            }
        }

        this.MusicSheetAssignmentService.SaveAsync();
        this.SetDialogResultAction.Invoke(true);
    }

    #endregion
}


public class PersonInfo : ObservableObject
{
    #region Fields

    private MusicSheet _selectedMusicSheet;

    #endregion


    #region Constructors

    public PersonInfo(Person person, IEnumerable<MusicSheet> assignableMusicSheets, MusicSheet selectedMusicSheet)
    {
        this.Person = person;
        this.AssignableMusicSheets = assignableMusicSheets;
        _selectedMusicSheet = selectedMusicSheet;
    }

    #endregion


    #region Properties

    public Person Person { get; }

    public IEnumerable<MusicSheet> AssignableMusicSheets { get; }

    public MusicSheet SelectedMusicSheet
    {
        get => _selectedMusicSheet;
        set => this.SetProperty(ref _selectedMusicSheet, value);
    }

    #endregion
}
