using HeadlessWebContainer.Services;
using HeadlessWebContainer.ViewModels;
using MaSch.Core;
using MaSch.Presentation.Wpf.Commands;
using MaSch.Presentation.Wpf.Common;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace HeadlessWebContainer.Views
{
    public partial class BrowserView : INotifyPropertyChanged
    {
        private readonly ISettingsService _settingsService;
        private string? _homePage;
        private bool _isLoading;
        private string? _address;

        public event PropertyChangedEventHandler? PropertyChanged;

        public BrowserViewModel ViewModel => (BrowserViewModel)DataContext;
        public ICommand BackCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand ReloadCommand { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }

        [DisallowNull]
        public string? Address
        {
            get => _address;
            set
            {
                _address = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Address)));
                WebBrowser.Source = GetUri(value);
            }
        }

        [DisallowNull]
        public string? HomePage
        {
            get => _homePage;
            set
            {
                _homePage = value;
                WebBrowser.Source = GetUri(value);
            }
        }

        public BrowserView(BrowserViewModel viewModel, ISettingsService settingsService)
        {
            _settingsService = Guard.NotNull(settingsService, nameof(settingsService));

            BackCommand = new DelegateCommand(() => WebBrowser.CanGoBack, () => WebBrowser.GoBack(), true);
            ForwardCommand = new DelegateCommand(() => WebBrowser.CanGoForward, () => WebBrowser.GoForward(), true);
            ReloadCommand = new DelegateCommand(() => WebBrowser.Reload());

            DataContext = viewModel;
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var settings = _settingsService.GuiSettings;
                WindowPosition.ApplyToWindow(settings.WindowPositions, this);
                ViewModel.IsTitlePinned = settings.IsTitlePinned;
                TitleButtons.SetWindowState(WindowState);
            };
            Closed += (s, e) =>
            {
                var settings = _settingsService.GuiSettings;
                settings.IsTitlePinned = ViewModel.IsTitlePinned;
                settings.BrowserHomeUrl = _homePage;
                settings.BrowserWindowTitle = Title;
                WindowPosition.AddWindowToList(settings.WindowPositions, this);
                _settingsService.SaveGuiSettings();
            };
            Activated += (s, e) => ViewModel.ForceShowTitle = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            Deactivated += (s, e) => ViewModel.ForceShowTitle = false;

            TitleButtons.CloseButtonClicked += Close;
            TitleButtons.MinimizeButtonClicked += () => WindowState = WindowState.Minimized;
            TitleButtons.NormalizeButtonClicked += () => WindowState = WindowState.Normal;
            TitleButtons.MaximizeButtonClicked += () => WindowState = WindowState.Maximized;
            StateChanged += (s, e) => TitleButtons.SetWindowState(WindowState);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5 && !e.IsRepeat)
            {
                WebBrowser.Reload();
            }
            else if ((e.Key == Key.LeftCtrl || e.Key == Key.Right) && !e.IsRepeat)
            {
                ViewModel.ForceShowTitle = true;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ViewModel.ForceShowTitle = false;
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser.Source = GetUri(HomePage!);
        }

        private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void WebBrowser_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            IsLoading = true;
        }

        private void WebBrowser_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            IsLoading = false;
        }

        private void WebBrowser_SourceChanged(object sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
        {
            _address = WebBrowser.Source.ToString();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Address)));
        }

        private static Uri GetUri(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return uri;
            return new Uri("https://" + url);
        }
    }
}
