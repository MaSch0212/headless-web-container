using HeadlessWebContainer.Services;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using MaSch.Presentation.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace HeadlessWebContainer.Tools
{
    public abstract class BaseTool : ICliExecutable
    {
        public static bool HasAnyToolsBeenExecuted { get; private set; }

        [CliCommandOption('T', "theme", Required = false, HelpText = "The theme to use (Json file).")]
        public string? ThemeFile { get; set; }

        [CliCommandOption('P', "profile", Required = false, HelpText = "The profile to use for this instance of the application.")]
        public virtual string? Profile { get; set; }

        public virtual string GetProfileName() => Profile ?? "<Default>";

        public int ExecuteCommand(CliExecutionContext context)
        {
            HasAnyToolsBeenExecuted = true;
            SetTheme(context);
            OnExecuteCommand(context);
            return 0;
        }

        protected abstract void OnExecuteCommand(CliExecutionContext context);

        private void SetTheme(CliExecutionContext context)
        {
            var settingsService = context.ServiceProvider.GetRequiredService<ISettingsService>();
            var theme = settingsService.GetOrUpdateTheme(ThemeFile);
            if (theme != null)
                ThemeManager.DefaultThemeManager.LoadTheme(theme);
            else
                ThemeManager.DefaultThemeManager.LoadTheme(Theme.FromDefaultTheme(DefaultTheme.Dark));
        }
    }
}
