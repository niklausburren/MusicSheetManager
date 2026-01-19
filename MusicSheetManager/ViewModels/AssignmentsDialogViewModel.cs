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
        this.RestoreDefaults = new RelayCommand(this.OnRestoreDefaults);
    }

    #endregion


    #region Properties

    public ICommand UpdateAssignments { get; }

    public ICommand RestoreDefaults { get; }

    private IPeopleService PeopleService { get; }

    private IMusicSheetAssignmentService MusicSheetAssignmentService { get; }

    public ObservableCollection<PersonInfo> People
    {
        get
        {
            if (_people == null)
            {
                _people = [];

                foreach (var person in this.PeopleService.People.Where(p => p.Instrument.Category != InstrumentCategory.Percussion))
                {
                    var assignableMusicSheets = this.MusicSheetAssignmentService.GetAssignableMusicSheets(this.MusicSheetFolder, person);
                    var defaultMusicSheet = this.MusicSheetAssignmentService.GetDefaultMusicSheet(this.MusicSheetFolder, person);
                    var assignedMusicSheet = this.MusicSheetAssignmentService.GetAssignedMusicSheet(this.MusicSheetFolder, person);

                    _people.Add(new PersonInfo(
                        person,
                        assignableMusicSheets,
                        assignedMusicSheet,
                        defaultMusicSheet));
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
                personInfo.SelectedMusicSheet != personInfo.DefaultMusicSheet)
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

    private void OnRestoreDefaults()
    {
        foreach (var personInfo in this.People)
        {
            personInfo.SelectedMusicSheet = personInfo.DefaultMusicSheet;
        }
    }

    #endregion
}


public class PersonInfo : ObservableObject
{
    #region Fields

    private MusicSheet _selectedMusicSheet;

    #endregion


    #region Constructors

    public PersonInfo(Person person, IEnumerable<MusicSheet> assignableMusicSheets, MusicSheet selectedMusicSheet, MusicSheet defaultMusicSheet)
    {
        this.Person = person;
        this.AssignableMusicSheets = assignableMusicSheets;
        this.DefaultMusicSheet = defaultMusicSheet;
        _selectedMusicSheet = selectedMusicSheet;
    }

    #endregion


    #region Properties

    public Person Person { get; }

    public IEnumerable<MusicSheet> AssignableMusicSheets { get; }

    public MusicSheet DefaultMusicSheet { get; }

    public MusicSheet SelectedMusicSheet
    {
        get { return _selectedMusicSheet; }
        set
        {
            if (this.SetProperty(ref _selectedMusicSheet, value))
            {
                this.OnPropertyChanged(nameof(this.IsCustomAssignment));
            }
        }
    }

    public bool IsCustomAssignment
    {
        get { return this.SelectedMusicSheet != null && this.SelectedMusicSheet != this.DefaultMusicSheet; }
    }

    #endregion
}
