using CefSharp;
using CefSharp.Wpf;
using CommandLine;
using HeadlessWebContainer.Models;
using HeadlessWebContainer.Views;
using MaSch.Core;
using MaSch.Presentation.Wpf;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = MaSch.Presentation.Wpf.MessageBox;

namespace HeadlessWebContainer
{
    public partial class App : Application
    {
        public static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaSch", "HeadlessWebContainer");
        public static readonly string GuiSettingsPath = Path.Combine(AppDataPath, "settings.gui.json");
        public static readonly string BrowserCachePath = Path.Combine(AppDataPath, "browser-cache");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ThemeManager.DefaultThemeManager.LoadTheme(Theme.FromDefaultTheme(DefaultTheme.Dark));
            InitializeServiceContext();

            var helpWriter = new StringWriter();
            var parser = new Parser(o =>
            {
                o.HelpWriter = helpWriter;
                o.MaximumDisplayWidth = int.MaxValue;
            });
            parser
                .ParseArguments(Environment.GetCommandLineArgs().Skip(1), typeof(RunOptions))
                .MapResult<RunOptions, int>(
                    o => Run(o, RunBrowser),
                    e => Run(e, _ => HandleError(helpWriter.ToString())));

            static int Run<T>(T value, Action<T> action)
            {
                action(value);
                return 0;
            }
        }

        private void RunBrowser(RunOptions options)
        {
            Directory.CreateDirectory(BrowserCachePath);
            var settings = new CefSettings()
            {
                CachePath = BrowserCachePath,
            };
            settings.CefCommandLineArgs.Add("persist_session_cookies", "1");
            Cef.Initialize(settings);

            ImageSource? icon = null;
            if (!string.IsNullOrEmpty(options.IconPath) && File.Exists(options.IconPath))
            {
                try
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(options.IconPath);
                    img.EndInit();
                    icon = img;
                }
                catch
                {
                }
            }

            var language = CultureInfo.GetCultureInfo("en-US");
            MainWindow = new BrowserView(options.Url)
            {
                Title = options.Title ?? string.Empty,
                Icon = icon,
                Language = XmlLanguage.GetLanguage(language.IetfLanguageTag),
            };
            MainWindow.Show();
        }

        private void HandleError(string errors)
        {
            new StartErrorView { ErrorMessage = errors }.ShowDialog();
            Shutdown(-1);
        }

        private static void InitializeServiceContext()
        {
            ServiceContext.Instance.AddService(GuiSettings.LoadSettings(GuiSettingsPath));
        }
    }
}
