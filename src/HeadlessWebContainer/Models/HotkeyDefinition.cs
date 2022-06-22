using MaSch.Core.Attributes;
using MaSch.Core.Observable;
using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
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
        string? Script { get; set; }
    }

    public partial class HotkeyDefinition : ObservableChangeTrackingObject, IHotkeyDefinition_Props
    {
    }
}
