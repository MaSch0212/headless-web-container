using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace HeadlessWebContainer.Services
{
    public interface IBrowserService
    {
        void InitializeBrowser();
        Window ShowBrowserWindow(string homeUrl, string? title, ImageSource? icon, CultureInfo? language);
    }
}