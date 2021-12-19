using HeadlessWebContainer.Views;
using MaSch.Console.Cli.Configuration;
using MaSch.Console.Cli.Runtime;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace HeadlessWebContainer.Tools
{
    [CliCommand("configure", IsDefault = true, HelpText = "Configure application.")]
    public class ConfigureTool : BaseTool
    {
        protected override void OnExecuteCommand(CliExecutionContext context)
        {
            var configurationView = context.ServiceProvider.GetRequiredService<ConfigurationView>();
            Application.Current.MainWindow = configurationView;
            configurationView.Show();
        }
    }
}
