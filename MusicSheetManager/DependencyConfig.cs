using Autofac;
using MusicSheetManager.Services;
using MusicSheetManager.ViewModels;
using MusicSheetManager.Views;

namespace MusicSheetManager;
public static class DependencyConfig
{
    #region Public Methods

    public static IContainer Configure()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<ImportDialogViewModel>().AsSelf();
        builder.RegisterType<ImportDialog>().AsSelf();

        builder.RegisterType<AssignmentsDialogViewModel>().AsSelf();
        builder.RegisterType<AssignmentsDialog>().AsSelf();

        builder.RegisterType<MusicSheetService>().As<IMusicSheetService>().SingleInstance();
        builder.RegisterType<MusicSheetAssignmentService>().As<IMusicSheetAssignmentService>().SingleInstance();
        builder.RegisterType<PeopleService>().As<IPeopleService>().SingleInstance();
        builder.RegisterType<PlaylistService>().As<IPlaylistService>().SingleInstance();
        builder.RegisterType<MusicSheetDistributionService>().As<IMusicSheetDistributionService>().SingleInstance();

        builder.RegisterType<MainWindowViewModel>().AsSelf();
        builder.RegisterType<MusicSheetTabViewModel>().AsSelf();
        builder.RegisterType<PeopleTabViewModel>().AsSelf();
        builder.RegisterType<PlaylistTabViewModel>().AsSelf();
        builder.RegisterType<ToolsViewModel>().AsSelf();
        builder.RegisterType<MainWindow>().AsSelf();

        return builder.Build();
    }

    #endregion
}