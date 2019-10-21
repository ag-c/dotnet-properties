﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Loader;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging.Serilog;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Styling;

using DotNet.Properties.Dialogs.Views;
using DotNet.Properties.Dialogs.ViewModels;
using DotNet.Properties.Services;
using DotNet.Properties.ViewModels;
using DotNet.Properties.Views;

namespace DotNet.Properties
{
    internal class App : Application
    {
        private static readonly List<FileDialogFilter> OpenProjectFileDialogFilters = new List<FileDialogFilter>()
        {
            new FileDialogFilter() { Name = "All Project Files", Extensions = new List<string>() { "*proj" } },
            new FileDialogFilter() { Name = "C# Project Files", Extensions = new List<string>() { "csproj" } },
            new FileDialogFilter() { Name = "Visual Basic Project Files", Extensions = new List<string>() { "vbproj" } },
            new FileDialogFilter() { Name = "F# Project Files", Extensions = new List<string>() { "fsproj" } },
        };

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();

            // workaround (TopLevel should apply styles when global styles change)
            Styles.CollectionChanged +=
                 (sender, e) =>
                 {
                     var dummyStyle = new Style();

                     foreach (var window in ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime).Windows)
                     {
                         window.Styles.Add(dummyStyle);
                         window.Styles.Remove(dummyStyle);
                     }
                 };

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();

                if (TryBuildMainWindowDataContext(mainWindow, out var mainWindowDataContext))
                {
                    mainWindow.DataContext = mainWindowDataContext;
                    desktop.MainWindow = mainWindow;
                }
            }
        }

        private static int Main(string[] args) =>
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);

        private bool TryBuildMainWindowDataContext(
            MainWindow mainWindow,
            out MainWindowViewModel? viewModel)
        {
            var projFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.*proj");

            var projectPath = projFiles.Length == 1 ? projFiles[0] : OpenProjectFile();
            var projectDirectory = Path.GetDirectoryName(projectPath);

            // TODO: replace with File.Exists and Directory.Exists when nullable annotations are correct
            if (!FileExists(projectPath) || !DirectoryExists(projectDirectory))
            {
                viewModel = null;
                return false;
            }

            IDotNetSdkResolver dotnetSdkResolver = new DotNetSdkResolver();

            if (!dotnetSdkResolver.TryResolveSdkPath(projectDirectory, out var dotnetSdkPath))
            {
                viewModel = null;
                return false;
            }

            var dotnetSdkPaths = new DotNetSdkPaths(dotnetSdkPath);

            IMSBuildLoader msBuildLoader = new MSBuildLoader(dotnetSdkPaths);

            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                if (name.Name != null && msBuildLoader.TryResolveMSBuildAssembly(context, name.Name, out var assembly))
                {
                    return assembly;
                }

                return null;
            };

            var msBuildProject = new MSBuildProject(dotnetSdkPaths, projectPath);

            viewModel = new MainWindowViewModel(
                msBuildProject,
                new DialogService<UnsavedChangesDialog, UnsavedChangesDialogViewModel>(NewUnsavedChangesDialog, mainWindow),
                new OpenFileDialogService(mainWindow),
                new ThemeService(this));

            return true;
        }

        private static string? OpenProjectFile() =>
            Task.Run(async () =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    AllowMultiple = false,
                    Filters = OpenProjectFileDialogFilters
                };

                var result = await openFileDialog.ShowAsync(null).ConfigureAwait(false);

                if (result != null && result.Length > 0)
                {
                    return result[0];
                }

                return null;
            }).Result;

        private static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if DEBUG
                .LogToDebug()
#endif
                .UseReactiveUI();

        private static UnsavedChangesDialog NewUnsavedChangesDialog() => new UnsavedChangesDialog();

        private static bool DirectoryExists([NotNullWhen(true)]string? path) => Directory.Exists(path);

        private static bool FileExists([NotNullWhen(true)]string? path) => File.Exists(path);
    }
}
