   using Autofac;
   using MusicSheetManager.Services;
   using MusicSheetManager.ViewModels;
   using MusicSheetManager.Views;

   namespace MusicSheetManager
   {
       public static class DependencyConfig
       {
           public static IContainer Configure()
           {
               var builder = new ContainerBuilder();

               builder.RegisterType<ImportViewModel>().AsSelf();
               builder.RegisterType<ImportDialog>().AsSelf();

               builder.RegisterType<MusicSheetImportService>().As<IMusicSheetImportService>();
               
               builder.RegisterType<MainWindow>().AsSelf();

               return builder.Build();
           }
       }
   }
   