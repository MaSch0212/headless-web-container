using HeadlessWebContainer.Services;
using MaSch.Console.Cli;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;

namespace HeadlessWebContainer.Tools
{
    [CliCommand("run", HelpText = "Run the application with a specific website.")]
    public class RunTool : BaseTool, ICliValidatable
    {
        [CliCommandOption('P', "profile", Required = false, HelpText = "The profile to use for this instance of the application.")]
        public override string? Profile { get; set; }

        [CliCommandOption('u', "url", Required = false, HelpText = "The URL to display in the headless browser.")]
        public string? Url { get; set; }

        [CliCommandOption('t', "title", Required = false, HelpText = "The title that the window should have.")]
        public string? Title { get; set; }

        [CliCommandOption('i', "icon", Required = false, HelpText = "The path to an icon to display in taskbar and title bar.")]
        public string? IconPath { get; set; }

        public override string GetProfileName() => Profile ?? Url ?? "<Default>";

        public bool ValidateOptions(CliExecutionContext context, [MaybeNullWhen(true)] out IEnumerable<CliError> errors)
        {
            var errorList = new List<CliError>();
            if (Profile is null && Url is null)
            {
                errorList.Add(new CliError("Either Profile or Url needs to be provided.", context.Command));
            }

            errors = errorList;
            return errorList.Count == 0;
        }

        protected override void OnExecuteCommand(CliExecutionContext context)
        {
            var browserService = context.ServiceProvider.GetRequiredService<IBrowserService>();
            var settingsService = context.ServiceProvider.GetRequiredService<ISettingsService>();

            var icon = settingsService.GetOrUpdateIcon(IconPath);
            var settings = settingsService.GuiSettings;
            Application.Current.MainWindow = browserService.ShowBrowserWindow(
                Url ?? settings.BrowserHomeUrl ?? string.Empty,
                Title ?? settings.BrowserWindowTitle,
                icon,
                null);
        }
    }
}
