using MaSch.Core.Attributes;
using MaSch.Core.Observable;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using System.Windows.Input;

namespace HeadlessWebContainer.Models
{
    [ObservablePropertyDefinition]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Property definition")]
    internal interface IHotkeyDefinition_Props
    {
        [JsonConverter(typeof(StringEnumConverter))]
        Keys Key { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        ModifierKeys ModifierKeys { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        HotkeyMode Mode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string? KeyPressScript { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string? KeyDownScript { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string? KeyUpScript { get; set; }
    }

    public partial class HotkeyDefinition : ObservableChangeTrackingObject, IHotkeyDefinition_Props
    {
        [JsonIgnore]
        [NoChangeTracking]
        public string? Script
        {
            get => KeyPressScript;
            set => KeyPressScript = value;
        }

        [NoChangeTracking]
        [JsonIgnore]
        [DependsOn(nameof(ModifierKeys))]
        public bool IsControl
        {
            get => ModifierKeys.HasFlag(ModifierKeys.Control);
            set => ModifierKeys ^= ModifierKeys.Control;
        }

        [NoChangeTracking]
        [JsonIgnore]
        [DependsOn(nameof(ModifierKeys))]
        public bool IsShift
        {
            get => ModifierKeys.HasFlag(ModifierKeys.Shift);
            set => ModifierKeys ^= ModifierKeys.Shift;
        }

        [NoChangeTracking]
        [JsonIgnore]
        [DependsOn(nameof(ModifierKeys))]
        public bool IsAlt
        {
            get => ModifierKeys.HasFlag(ModifierKeys.Alt);
            set => ModifierKeys ^= ModifierKeys.Alt;
        }

        [NoChangeTracking]
        [JsonIgnore]
        [DependsOn(nameof(ModifierKeys))]
        public bool IsWindows
        {
            get => ModifierKeys.HasFlag(ModifierKeys.Windows);
            set => ModifierKeys ^= ModifierKeys.Windows;
        }
    }

    public enum HotkeyMode
    {
        KeyPress,
        KeyUpDown,
    }
}
