using CefSharp;
using CefSharp.Wpf;
using HeadlessWebContainer.Views;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace HeadlessWebContainer.Services
{
    public class BrowserService : IBrowserService
    {
        private readonly string _browserCachePath;

        public BrowserService(string browserCachePath)
        {
            _browserCachePath = browserCachePath;
        }

        public void InitializeBrowser()
        {
            Directory.CreateDirectory(_browserCachePath);
            var settings = new CefSettings()
            {
                CachePath = _browserCachePath,
            };
            settings.CefCommandLineArgs.Add("persist_session_cookies", "1");
            Cef.Initialize(settings);
        }

        public Window ShowBrowserWindow(string homeUrl, string? title, ImageSource? icon, CultureInfo? language)
        {
            language ??= CultureInfo.GetCultureInfo("en-US");
            var result = new BrowserView(homeUrl)
            {
                Title = title ?? string.Empty,
                Icon = icon,
                Language = XmlLanguage.GetLanguage(language.IetfLanguageTag),
            };
            result.Show();
            return result;
        }
    }
}
