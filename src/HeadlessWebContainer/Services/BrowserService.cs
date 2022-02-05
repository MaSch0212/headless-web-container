using HeadlessWebContainer.Views;
using MaSch.Core;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace HeadlessWebContainer.Services
{
    public class BrowserService : IBrowserService
    {
        private readonly BrowserView _browserView;

        public BrowserService(BrowserView browserView)
        {
            _browserView = Guard.NotNull(browserView, nameof(browserView));
        }

        public Window ShowBrowserWindow(string homeUrl, string? title, ImageSource? icon, CultureInfo? language)
        {
            language ??= CultureInfo.GetCultureInfo("en-US");
            _browserView.Title = title ?? string.Empty;
            _browserView.Icon = icon;
            _browserView.Language = XmlLanguage.GetLanguage(language.IetfLanguageTag);
            _browserView.Show(homeUrl);
            return _browserView;
        }
    }
}
