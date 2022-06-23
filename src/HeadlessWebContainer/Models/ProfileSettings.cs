using MaSch.Core.Attributes;
using MaSch.Core.Observable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HeadlessWebContainer.Models
{
    [ObservablePropertyDefinition]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Property definition")]
    internal interface IObservableObject_Props
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }
        public BitmapImage? Icon { get; set; }

        public bool UseDarkTheme { get; set; }
        public Color HighlightColor { get; set; }
        public Color HighlightContrastColor { get; set; }
    }

    public partial class ProfileSettings : ObservableChangeTrackingObject, IObservableObject_Props
    {
        public string? ProfilePath { get; set; }
        public string? IconFilePath { get; set; }
        public bool IsNewProfile { get; set; }

        [RecursiveChangeTracking]
        public ObservableCollection<HotkeyDefinition> Hotkeys { get; set; } = new();
    }
}
