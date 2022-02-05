using HeadlessWebContainer.Services;
using HeadlessWebContainer.Tools;
using HeadlessWebContainer.ViewModels;
using HeadlessWebContainer.Views;
using MaSch.Console.Cli;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace HeadlessWebContainer
{
    public partial class App : Application
    {
        internal static readonly string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MaSch", "HeadlessWebContainer");
        internal static readonly string BrowserCachePath = Path.Combine(AppDataPath, "browser-cache-wv2");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            var consoleWriter = new StringWriter();
            Console.SetOut(consoleWriter);

            var app = new CliApplicationBuilder()
                .WithCommand<ConfigureTool>()
                .WithCommand<RunTool>()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IFileSystemService, FileSystemService>();
                    services.AddScoped<IBrowserService, BrowserService>();
                    services.AddScoped<ISettingsService, SettingsService>();

                    services.AddTransient<ConfigurationViewModel>();
                    services.AddTransient<ConfigurationView>();
                    services.AddTransient<BrowserViewModel>();
                    services.AddTransient<BrowserView>();
                })
                .Build();

            _ = app.Run(Environment.GetCommandLineArgs().Skip(1).ToArray());
            if (!BaseTool.HasAnyToolsBeenExecuted)
            {
                var consoleOutput = string.Join(
                    Environment.NewLine,
                    consoleWriter.ToString()
                        .Replace("\r", string.Empty)
                        .Split('\n')
                        .Select(x => x.TrimEnd()));
                new StartErrorView(consoleOutput).ShowDialog();
                Shutdown(-1);
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
                MessageBox.Show(e.Exception.ToString());
            }
        }
    }
}
