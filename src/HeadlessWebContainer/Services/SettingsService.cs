using HeadlessWebContainer.Models;
using MaSch.Core;
using MaSch.Core.Extensions;
using MaSch.Presentation.Wpf;
using MaSch.Presentation.Wpf.Themes;
using MaSch.Presentation.Wpf.ThemeValues;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace HeadlessWebContainer.Services
{
    public class SettingsService : ISettingsService
    {
        private const string GuiSettingsFileName = "settings.gui.json";

        private readonly IFileSystemService _fileSystemService;
        private readonly string _profileIndexPath;
        private readonly string _profileDirPath;
        private readonly string _guiSettingsPath;
        private GuiSettings? _guiSettings;

        public GuiSettings GuiSettings
            => _guiSettings ??= _fileSystemService.LoadJsonFromFile<GuiSettings>(_guiSettingsPath, () => new());

        public SettingsService(string profileIndexPath, string profile)
        {
            ServiceContext.GetService(out _fileSystemService);

            _profileIndexPath = profileIndexPath;
            _profileDirPath = GetProfilePath(GetProfileId(profile));
            _guiSettingsPath = Path.Combine(_profileDirPath, GuiSettingsFileName);

            Directory.CreateDirectory(_profileDirPath);
        }

        public void SaveGuiSettings()
            => _fileSystemService.SaveJsonToFile(_guiSettingsPath, GuiSettings);

        public BitmapImage? GetOrUpdateIcon(string? newIconPath)
        {
            var iconFile = Path.Combine(_profileDirPath, "icon.ico");
            if (_fileSystemService.TryLoadImage(newIconPath, out var result))
            {
                var newIconHash = _fileSystemService.GetFileHash(newIconPath);
                if (!File.Exists(iconFile) || (GuiSettings.IconHash != newIconHash && _fileSystemService.GetFileHash(iconFile) != newIconHash))
                {
                    GuiSettings.IconHash = newIconHash;
                    SaveGuiSettings();

                    if (File.Exists(iconFile))
                        File.Delete(iconFile);

                    var bmp = BitmapImage2Bitmap(result);
                    bmp.Save(iconFile, ImageFormat.Icon);
                }
            }
            else
            {
                if (!_fileSystemService.TryLoadImage(iconFile, out result))
                {
                    result = _fileSystemService.LoadDefaultImage();
                    _fileSystemService.CopyDefaultImage(_profileDirPath);
                }
            }

            return result;
        }

        public ITheme? GetOrUpdateTheme(string? themeFilePath)
        {
            var profileThemePath = Path.Combine(_profileDirPath, "theme.json");
            if (_fileSystemService.TryLoadTheme(themeFilePath, out var result))
            {
                if (!File.Exists(profileThemePath) || _fileSystemService.GetFileHash(profileThemePath) != _fileSystemService.GetFileHash(themeFilePath))
                    File.Copy(themeFilePath, profileThemePath, true);
            }
            else if (File.Exists(profileThemePath))
            {
                _ = _fileSystemService.TryLoadTheme(profileThemePath, out result);
            }

            return result;
        }

        public IEnumerable<ProfileSettings> GetAllProfiles()
        {
            var tm = ThemeManager.DefaultThemeManager;
            var defaultThemeInfo = (
                true,
                tm.GetValue<Color>(ThemeKey.HighlightColor)!.Value,
                tm.GetValue<Color>(ThemeKey.HoverHighlightColor)!.Value,
                tm.GetValue<Color>(ThemeKey.HighlightContrastColor)!.Value,
                tm.GetValue<Color>(ThemeKey.HoverHighlightContrastColor)!.Value);

            var profileIndex = _fileSystemService.LoadJsonFromFile<Dictionary<string, string>>(_profileIndexPath, () => new());
            foreach (var p in profileIndex)
            {
                var path = GetProfilePath(p.Value);
                var guiSettings = _fileSystemService.LoadJsonFromFile<GuiSettings>(Path.Combine(path, GuiSettingsFileName), () => new());

                var iconFilePath = Path.Combine(path, "icon.ico");
                bool hasIcon = _fileSystemService.TryLoadImage(iconFilePath, out var icon);

                var themeFilePath = Path.Combine(path, "theme.json");
                var themeInfo = defaultThemeInfo;
                if (_fileSystemService.TryLoadTheme(themeFilePath, out var theme))
                {
                    themeInfo = (
                        ((Color)theme.Values[nameof(ThemeKey.NormalBackgroundColor)].RawValue!) == tm.GetValue<Color>(ThemeKey.NormalBackgroundColor)!.Value,
                        ((ColorThemeValue)theme.Values[nameof(ThemeKey.HighlightColor)]).Value,
                        ((ColorThemeValue)theme.Values[nameof(ThemeKey.HoverHighlightColor)]).Value,
                        ((ColorThemeValue)theme.Values[nameof(ThemeKey.HighlightContrastColor)]).Value,
                        ((ColorThemeValue)theme.Values[nameof(ThemeKey.HoverHighlightContrastColor)]).Value);
                }

                yield return new ProfileSettings
                {
                    Name = p.Key,
                    Title = guiSettings.BrowserWindowTitle,
                    Url = guiSettings.BrowserHomeUrl,
                    Icon = icon,

                    UseDarkTheme = themeInfo.Item1,
                    HighlightColor = themeInfo.Item2,
                    HoverHighlightColor = themeInfo.Item3,
                    HighlightContrastColor = themeInfo.Item4,
                    HoverHighlightContrastColor = themeInfo.Item5,

                    ProfilePath = path,
                    IconFilePath = hasIcon ? iconFilePath : null,
                };
            }
        }

        public void SaveAllProfiles(IEnumerable<ProfileSettings> profiles)
        {
            var profileIndex = _fileSystemService.LoadJsonFromFile<Dictionary<string, string>>(_profileIndexPath, () => new());

            var profilesToDelete = profileIndex.Keys.Except(profiles.Select(x => x.Name!)).ToArray();
            foreach (var p in profilesToDelete)
            {
                Directory.Delete(GetProfilePath(profileIndex[p]));
                profileIndex.Remove(p);
            }

            profileIndex.Add(
                profiles
                    .Select(x => x.Name!)
                    .Except(profileIndex.Keys)
                    .Select(x => new KeyValuePair<string, string>(x, BitConverter.GetBytes(x.GetHashCode()).ToHexString())));

            foreach (var p in profileIndex)
            {
                var profileSettings = profiles.Single(x => x.Name == p.Key);
                var path = GetProfilePath(p.Value);
                Directory.CreateDirectory(path);
                var guiSettings = _fileSystemService.LoadJsonFromFile<GuiSettings>(Path.Combine(path, GuiSettingsFileName), () => new());

                if (!string.IsNullOrEmpty(profileSettings.IconFilePath) && profileSettings.Icon != null)
                {
                    var iconFilePath = Path.Combine(path, "icon.ico");
                    var iconHash = _fileSystemService.GetFileHash(profileSettings.IconFilePath);
                    if (!File.Exists(iconFilePath) || (guiSettings.IconHash != iconHash && _fileSystemService.GetFileHash(iconFilePath) != iconHash))
                    {
                        if (File.Exists(iconFilePath))
                            File.Delete(iconFilePath);
                        if (string.Equals(Path.GetExtension(profileSettings.IconFilePath), ".ico", StringComparison.OrdinalIgnoreCase))
                            File.Copy(profileSettings.IconFilePath, iconFilePath);
                        else
                            BitmapImage2Bitmap(profileSettings.Icon).Save(iconFilePath, ImageFormat.Icon);
                        if (profileSettings.IconFilePath != iconFilePath)
                            guiSettings.IconHash = iconHash;
                    }
                }
                else
                {
                    _fileSystemService.CopyDefaultImage(path);
                }

                guiSettings.BrowserHomeUrl = profileSettings.Url;
                guiSettings.BrowserWindowTitle = profileSettings.Title;
                _fileSystemService.SaveJsonToFile(Path.Combine(path, GuiSettingsFileName), guiSettings);

                if (profileSettings.ChangeTracker.HasPropertyChanged(nameof(ProfileSettings.UseDarkTheme)) ||
                    profileSettings.ChangeTracker.HasPropertyChanged(nameof(ProfileSettings.HighlightColor)) ||
                    profileSettings.ChangeTracker.HasPropertyChanged(nameof(ProfileSettings.HighlightContrastColor)) ||
                    profileSettings.ChangeTracker.HasPropertyChanged(nameof(ProfileSettings.HoverHighlightColor)) ||
                    profileSettings.ChangeTracker.HasPropertyChanged(nameof(ProfileSettings.HoverHighlightContrastColor)))
                {
                    var themeFilePath = Path.Combine(path, "theme.json");
                    var theme = new
                    {
                        MergedThemes = new string[]
                        {
                            "#DefaultThemes/" + (profileSettings.UseDarkTheme ? "Dark" : "Light"),
                        },
                        Values = new
                        {
                            HighlightColor = new
                            {
                                Type = "Color",
                                Value = Color2Hex(profileSettings.HighlightColor),
                            },
                            HighlightContrastColor = new
                            {
                                Type = "Color",
                                Value = Color2Hex(profileSettings.HighlightContrastColor),
                            },
                            HoverHighlightColor = new
                            {
                                Type = "Color",
                                Value = Color2Hex(profileSettings.HoverHighlightColor),
                            },
                            HoverHighlightContrastColor = new
                            {
                                Type = "Color",
                                Value = Color2Hex(profileSettings.HoverHighlightContrastColor),
                            },
                        },
                    };
                    _fileSystemService.SaveJsonToFile(themeFilePath, theme);
                }
            }

            _fileSystemService.SaveJsonToFile(_profileIndexPath, profileIndex);
        }

        private string GetProfileId(string profile)
        {
            var profileIndex = _fileSystemService.LoadJsonFromFile<Dictionary<string, string>>(_profileIndexPath, () => new());
            if (!profileIndex.TryGetValue(profile, out var profileId))
            {
                profileId = BitConverter.GetBytes(profile.GetHashCode()).ToHexString();
                profileIndex.Add(profile, profileId);
                _fileSystemService.SaveJsonToFile(_profileIndexPath, profileIndex);
            }

            return profileId;
        }

        private string GetProfilePath(string profileId)
            => Path.Combine(Path.GetDirectoryName(_profileIndexPath)!, "profiles", profileId);

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using var outStream = new MemoryStream();

            var enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            enc.Save(outStream);
            var bitmap = new Bitmap(outStream);

            return new Bitmap(bitmap);
        }

        private string Color2Hex(Color color)
            => "#" + new byte[] { color.R, color.G, color.B }.ToHexString().ToUpperInvariant();
    }
}
