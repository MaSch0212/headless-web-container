using MaSch.Presentation.Wpf.Common;
using System.Collections.Generic;

namespace HeadlessWebContainer.Models
{
    public class GuiSettings
    {
        public List<WindowPosition> WindowPositions { get; set; }
        public bool IsTitlePinned { get; set; }
        public string? BrowserWindowTitle { get; set; }
        public string? BrowserHomeUrl { get; set; }
        public string? IconHash { get; set; }

        public GuiSettings()
        {
            WindowPositions = new List<WindowPosition>();
            IsTitlePinned = true;
        }
    }
}
