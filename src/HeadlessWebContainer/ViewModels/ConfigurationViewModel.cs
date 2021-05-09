using HeadlessWebContainer.Models;
using HeadlessWebContainer.Services;
using HeadlessWebContainer.Views;
using MaSch.Core;
using MaSch.Core.Attributes;
using MaSch.Core.Extensions;
using MaSch.Core.Observable;
using MaSch.Core.Observable.Modules;
using MaSch.Presentation.Wpf;
using MaSch.Presentation.Wpf.Commands;
using MaSch.Presentation.Wpf.Services;
using MaSch.Presentation.Wpf.Themes;
using Microsoft.Win32;
using ShellLink;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MessageBox = MaSch.Presentation.Wpf.MessageBox;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace HeadlessWebContainer.ViewModels
{
    [ObservablePropertyDefinition]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Property definition")]
    internal interface IConfigurationViewModel_Props
    {
        ProfileSettings? SelectedProfile { get; set; }

        [ObservablePropertyAccessModifier(SetModifier = AccessModifier.Private)]
        bool IsLoading { get; }
    }

    public partial class ConfigurationViewModel : ObservableObject, IConfigurationViewModel_Props
    {
        private readonly IFileSystemService _fileSystemService;
        private readonly ISettingsService _settingsService;
        private readonly IChangeTracker _changeTracker;

        public bool HasChanges => _changeTracker.HasChanges;

        public ObservableCollection<ProfileSettings> Profiles { get; }
        public IStatusMessageService StatusService { get; }
        [DependsOn(nameof(IsLoading))]
        public ICommand UndoCommand { get; }
        [DependsOn(nameof(IsLoading))]
        public ICommand SaveCommand { get; }
        [DependsOn(nameof(IsLoading))]
        public ICommand CreateProfileCommand { get; }
        [DependsOn(nameof(IsLoading))]
        public ICommand DeleteProfileCommand { get; }
        [DependsOn(nameof(IsLoading))]
        public ICommand ChangeProfileIconCommand { get; }
        [DependsOn(nameof(IsLoading))]
        public ICommand CreateProfileShortcutCommand { get; }

        public ConfigurationViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                ServiceContext.GetService(out _fileSystemService);
                ServiceContext.GetService(out _settingsService);

                Profiles = new ObservableCollection<ProfileSettings>(_settingsService.GetAllProfiles());
                StatusService = new StatusMessageService();
                StatusService.StatusChanged += (s, e) => IsLoading = e.Status == StatusType.Loading;

                _changeTracker = new ChangeTracker(typeof(ConfigurationViewModel), true);
                _changeTracker.SetBaseValue(Profiles, nameof(Profiles));
                _changeTracker.ResetChangeTracking();
            }

            UndoCommand = new AsyncDelegateCommand(() => !IsLoading, Undo);
            SaveCommand = new AsyncDelegateCommand(() => !IsLoading, Save);
            CreateProfileCommand = new DelegateCommand(() => !IsLoading, CreateProfile);
            DeleteProfileCommand = new DelegateCommand<ProfileSettings>(x => x != null && !IsLoading, DeleteProfile);
            ChangeProfileIconCommand = new DelegateCommand<ProfileSettings>(x => x != null && !IsLoading, ChangeProfileIcon);
            CreateProfileShortcutCommand = new DelegateCommand<ProfileSettings>(x => x != null && !x.IsNewProfile && !IsLoading, CreateProfileShortcut);
        }

        public void ResetChangeTracking() => _changeTracker.ResetChangeTracking();

        private async Task Undo()
        {
            StatusService.StartLoading("Reverting settings...");
            try
            {
                if (_changeTracker.HasChanges)
                {
                    var selectedProfileName = SelectedProfile?.Name;

                    Profiles.Clear();
                    var profiles = await Task.Run(() => _settingsService.GetAllProfiles()).ConfigureAwait(true);
                    Profiles.Add(profiles);

                    if (selectedProfileName != null)
                        SelectedProfile = Profiles.FirstOrDefault(x => x.Name == selectedProfileName);

                    _changeTracker.ResetChangeTracking();
                    StatusService.PushSuccess("Successfully reverted settings");
                }
                else
                {
                    StatusService.PushInformation("Nothing has changed");
                }
            }
            catch (Exception ex)
            {
                new StartErrorView(ex.ToString()).Show();
                StatusService.PushError("Failed to revert settings");
            }
        }

        public async Task Save()
        {
            StatusService.StartLoading("Saving settings...");
            try
            {
                if (_changeTracker.HasChanges)
                {
                    var selectedProfileName = SelectedProfile?.Name;

                    await Task.Run(() => _settingsService.SaveAllProfiles(Profiles));

                    Profiles.Clear();
                    var profiles = await Task.Run(() => _settingsService.GetAllProfiles()).ConfigureAwait(true);
                    Profiles.Add(profiles);

                    if (selectedProfileName != null)
                        SelectedProfile = Profiles.FirstOrDefault(x => x.Name == selectedProfileName);

                    _changeTracker.ResetChangeTracking();
                    StatusService.PushSuccess("Successfully saved settings");
                }
                else
                {
                    StatusService.PushInformation("Nothing has changed");
                }
            }
            catch (Exception ex)
            {
                new StartErrorView(ex.ToString()).Show();
                StatusService.PushError("Failed to save settings");
            }
        }

        private void CreateProfile()
        {
            StatusService.StartLoading("Creating profile...");

            var inputMsgBox = new InputMessageBox
            {
                Title = "Create Profile",
                Message = "Type in the name of the profile:",
                ValidationFunction = x =>
                {
                    if (Profiles.Any(y => y.Name == x))
                        return $"A profile with the name \"{x}\" already exists. Please pick another name.";
                    return null;
                },
            };
            if (inputMsgBox.ShowDialog() == true)
            {
                var tm = ThemeManager.DefaultThemeManager;
                var profile = new ProfileSettings
                {
                    Name = inputMsgBox.SelectedText,
                    UseDarkTheme = true,
                    HighlightColor = tm.GetValue<Color>(ThemeKey.HighlightColor)!.Value,
                    HoverHighlightColor = tm.GetValue<Color>(ThemeKey.HoverHighlightColor)!.Value,
                    HighlightContrastColor = tm.GetValue<Color>(ThemeKey.HighlightContrastColor)!.Value,
                    HoverHighlightContrastColor = tm.GetValue<Color>(ThemeKey.HoverHighlightContrastColor)!.Value,
                    Icon = _fileSystemService.LoadDefaultImage(),
                    IsNewProfile = true,
                };
                Profiles.Add(profile);
                SelectedProfile = profile;
                StatusService.PushSuccess($"Successfully created profile \"{profile.Name}\"");
            }
            else
            {
                StatusService.Clear();
            }
        }

        private void DeleteProfile(ProfileSettings? profile)
        {
            if (profile == null)
                return;

            StatusService.StartLoading("Deleting profile...");

            var r = MessageBox.Show($"Do you really want to delete the profile \"{profile.Name}\"?", "Delete profile", MessageBoxButton.YesNo);
            if (r == MessageBoxResult.Yes)
            {
                Profiles.Remove(profile);
                StatusService.PushSuccess($"Successfully deleted profile \"{profile.Name}\"");
            }
            else
            {
                StatusService.Clear();
            }
        }

        private void ChangeProfileIcon(ProfileSettings? profile)
        {
            if (profile == null)
                return;

            var ofd = new OpenFileDialog
            {
                Filter = "Icon files (*.ico)|*.ico|Image files (*.bmp;*.jpg;*.gif;*.png)|*.bmp;*.jpg;*.gif;*.png|All files (*.*)|*.*",
            };
            if (ofd.ShowDialog() == true)
            {
                if (_fileSystemService.TryLoadImage(ofd.FileName, out var image))
                {
                    profile.Icon = image;
                    profile.IconFilePath = ofd.FileName;
                }
                else
                {
                    StatusService.PushError("Failed to load image file");
                }
            }
        }

        private void CreateProfileShortcut(ProfileSettings? profile)
        {
            if (profile == null)
                return;

            var startMenuDir = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            var sfd = new SaveFileDialog
            {
                Filter = "Shortcut files (*.lnk)|*.lnk",
                InitialDirectory = startMenuDir,
                FileName = (string.IsNullOrEmpty(profile.Title) ? profile.Name : profile.Title) + ".lnk",
            };
            if (sfd.ShowDialog() == true)
            {
                var shortcut = Shortcut.CreateShortcut(
                    Process.GetCurrentProcess().MainModule!.FileName,
                    $"run -P \"{profile.Name}\"",
                    Path.Combine(profile.ProfilePath!, "icon.ico"),
                    0);
                shortcut.WriteToFile(sfd.FileName);
            }
        }
    }
}
