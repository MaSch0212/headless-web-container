using MaSch.Core.Attributes;
using MaSch.Core.Observable;
using System.Diagnostics.CodeAnalysis;

namespace HeadlessWebContainer.ViewModels
{
    [ObservablePropertyDefinition]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Property definition")]
    internal interface IBrowserViewModel_Props
    {
        bool IsTitlePinned { get; set; }
        bool ForceShowTitle { get; set; }
    }

    public partial class BrowserViewModel : ObservableObject, IBrowserViewModel_Props
    {
        [DependsOn(nameof(IsTitlePinned), nameof(ForceShowTitle))]
        public bool IsTitleVisible => IsTitlePinned || ForceShowTitle;
    }
}
