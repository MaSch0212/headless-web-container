using HeadlessWebContainer.Services;
using HeadlessWebContainer.ViewModels;
using MaSch.Core;
using MaSch.Presentation.Wpf.Common;
using System.ComponentModel;
using System.Windows;
using MessageBox = MaSch.Presentation.Wpf.MessageBox;

namespace HeadlessWebContainer.Views
{
    public partial class ConfigurationView
    {
        private readonly ISettingsService _settingsService;

        public ConfigurationViewModel ViewModel => (ConfigurationViewModel)DataContext;

        public ConfigurationView()
        {
            ServiceContext.GetService(out _settingsService);

            DataContext = new ConfigurationViewModel();
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var settings = _settingsService.GuiSettings;
                WindowPosition.ApplyToWindow(settings.WindowPositions, this);
                ViewModel.ResetChangeTracking();
            };
            Closed += (s, e) =>
            {
                var settings = _settingsService.GuiSettings;
                WindowPosition.AddWindowToList(settings.WindowPositions, this);
                _settingsService.SaveGuiSettings();
            };
            Closing += ConfigurationView_Closing;
        }

        private async void ConfigurationView_Closing(object sender, CancelEventArgs e)
        {
            if (ViewModel.IsLoading)
            {
                e.Cancel = true;
                return;
            }

            if (ViewModel.HasChanges)
            {
                var r = MessageBox.Show("You have made changes. Do you want to save them now?", "Headless Web Container", MessageBoxButton.YesNoCancel);
                if (r == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    await ViewModel.Save();
                    Close();
                }
                else if (r == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
