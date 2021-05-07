using CommandLine;

#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace HeadlessWebContainer
{
    [Verb("run", HelpText = "Run the application with a specific website.")]
    public class RunOptions
    {
        [Option('u', "url", Required = true, HelpText = "The URL to display in the headless browser.")]
        public string Url { get; set; }

        [Option('t', "title", Required = false, HelpText = "The title that the window should have.")]
        public string? Title { get; set; }

        [Option('i', "icon", Required = false, HelpText = "The path to an icon to display in taskbar and title bar.")]
        public string? IconPath { get; set; }
    }
}
