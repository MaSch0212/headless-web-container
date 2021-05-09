using HeadlessWebContainer.Models;
using MaSch.Presentation.Wpf;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace HeadlessWebContainer.Services
{
    public interface ISettingsService
    {
        GuiSettings GuiSettings { get; }

        BitmapImage? GetOrUpdateIcon(string? newIconPath);
        ITheme? GetOrUpdateTheme(string? themeFilePath);
        void SaveGuiSettings();

        IEnumerable<ProfileSettings> GetAllProfiles();
        void SaveAllProfiles(IEnumerable<ProfileSettings> profiles);
    }
}