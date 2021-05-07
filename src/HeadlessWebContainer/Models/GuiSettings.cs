using MaSch.Core.Observable;
using MaSch.Presentation.Wpf.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace HeadlessWebContainer.Models
{
    public class GuiSettings : ObservableObject
    {
        private static readonly JsonSerializerSettings JsonSettings;

        public List<WindowPosition> WindowPositions { get; set; }
        public bool IsTitlePinned { get; set; }

        [JsonIgnore]
        public string? SettingsFilePath { get; private set; }

        static GuiSettings()
        {
            JsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            JsonSettings.Converters.Add(new StringEnumConverter());
        }

        public GuiSettings()
        {
            WindowPositions = new List<WindowPosition>();
            IsTitlePinned = true;
        }

        public void SaveSettings()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath));
            File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(this, JsonSettings));
        }

        public void ApplyToWindow(Window window, bool disableMinimize = true) => WindowPosition.ApplyToWindow(WindowPositions, window, disableMinimize);
        public void UpdateFromWindow(Window window, bool disableMinimize = true)
        {
            WindowPosition.AddWindowToList(WindowPositions, window, disableMinimize);
            SaveSettings();
        }

        public static GuiSettings LoadSettings(string filePath)
        {
            GuiSettings result;
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                result = JsonConvert.DeserializeObject<GuiSettings>(json, JsonSettings) ?? new GuiSettings();
            }
            else
            {
                result = new GuiSettings();
            }

            result.SettingsFilePath = filePath;
            return result;
        }
    }
}
