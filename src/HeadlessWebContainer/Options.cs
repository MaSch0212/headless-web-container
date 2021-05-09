using CommandLine;

#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace HeadlessWebContainer
{
    public abstract class CommonOptions
    {
        [Option('T', "theme", Required = false, HelpText = "The theme to use (Json file).")]
        public string? ThemeFile { get; set; }

        [Option('P', "profile", Required = false, HelpText = "The profile to use for this instance of the application.")]
        public virtual string? Profile { get; set; }

        public virtual string GetProfileName() => Profile ?? "<Default>";
    }

    [Verb("configure", isDefault: true, HelpText = "Configure application.")]
    public class ConfigureOptions : CommonOptions
    {
    }

    [Verb("run", HelpText = "Run the application with a specific website.")]
    public class RunOptions : CommonOptions
    {
        [Option('P', "profile", Required = true, HelpText = "The profile to use for this instance of the application.", SetName = "profile")]
        public override string? Profile { get; set; }

        [Option('u', "url", Required = true, HelpText = "The URL to display in the headless browser.", SetName = "url")]
        public string? Url { get; set; }

        [Option('t', "title", Required = false, HelpText = "The title that the window should have.")]
        public string? Title { get; set; }

        [Option('i', "icon", Required = false, HelpText = "The path to an icon to display in taskbar and title bar.")]
        public string? IconPath { get; set; }

        public override string GetProfileName() => Profile ?? Url ?? "<Default>";
    }
}
