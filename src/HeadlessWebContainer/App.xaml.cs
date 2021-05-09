using CommandLine;
using HeadlessWebContainer.Services;
using HeadlessWebContainer.Views;
using MaSch.Core;
using MaSch.Presentation.Wpf;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace HeadlessWebContainer
{
    public partial class App : Application
    {
        internal static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaSch", "HeadlessWebContainer");
        internal static readonly string BrowserCachePath = Path.Combine(AppDataPath, "browser-cache");
        internal static readonly string ProfileIndexPath = Path.Combine(AppDataPath, "profile-index.json");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            var helpWriter = new StringWriter();
            var parser = new Parser(o =>
            {
                o.HelpWriter = helpWriter;
                o.MaximumDisplayWidth = int.MaxValue;
            });
            parser
                .ParseArguments(Environment.GetCommandLineArgs().Skip(1), typeof(ConfigureOptions), typeof(RunOptions))
                .MapResult<ConfigureOptions, RunOptions, int>(
                    o => Run(o, RunConfigure),
                    o => Run(o, RunBrowser),
                    e => Run(e, _ => HandleError(helpWriter.ToString())));

            static int Run<T>(T value, Action<T> action)
            {
                var options = value as CommonOptions;
                InitializeServiceContext(options?.GetProfileName() ?? "<Default>");
                SetTheme(options?.ThemeFile);

                action(value);
                return 0;
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                new StartErrorView(e.Exception.ToString()).ShowDialog();
                e.Handled = true;
            }
            catch
            {
                System.Windows.MessageBox.Show(e.Exception.ToString());
            }
        }

        private void RunConfigure(ConfigureOptions options)
        {
            MainWindow = new ConfigurationView();
            MainWindow.Show();
        }

        private void RunBrowser(RunOptions options)
        {
            var settingsService = ServiceContext.GetService<ISettingsService>();
            var browserService = ServiceContext.GetService<IBrowserService>();

            var icon = settingsService.GetOrUpdateIcon(options.IconPath);
            var settings = settingsService.GuiSettings;
            browserService.InitializeBrowser();
            MainWindow = browserService.ShowBrowserWindow(
                options.Url ?? settings.BrowserHomeUrl ?? string.Empty,
                options.Title ?? settings.BrowserWindowTitle,
                icon,
                null);
        }

        private void HandleError(string errors)
        {
            new StartErrorView(errors).ShowDialog();
            Shutdown(-1);
        }

        private static void SetTheme(string? themeFile)
        {
            var settingsService = ServiceContext.GetService<ISettingsService>();

            var theme = settingsService.GetOrUpdateTheme(themeFile);
            if (theme != null)
                ThemeManager.DefaultThemeManager.LoadTheme(theme);
            else
                ThemeManager.DefaultThemeManager.LoadTheme(Theme.FromDefaultTheme(DefaultTheme.Dark));
        }

        private static void InitializeServiceContext(string profile)
        {
            ServiceContext.AddService<IFileSystemService>(new FileSystemService());
            ServiceContext.AddService<ISettingsService>(new SettingsService(ProfileIndexPath, profile));
            ServiceContext.AddService<IBrowserService>(new BrowserService(BrowserCachePath));
        }
    }
}
