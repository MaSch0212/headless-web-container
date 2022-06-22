using HeadlessWebContainer.Services;
using HeadlessWebContainer.ViewModels;
using MaSch.Core;
using MaSch.Native.Windows.Input;
using MaSch.Presentation.Wpf.Commands;
using MaSch.Presentation.Wpf.Common;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Keys = System.Windows.Forms.Keys;

namespace HeadlessWebContainer.Views
{
    public partial class BrowserView : INotifyPropertyChanged
    {
        private readonly ISettingsService _settingsService;
        private Uri? _homePage;
        private bool _isLoading;
        private string? _address;
        private bool _windowMoved = false;

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

        public BrowserView(BrowserViewModel viewModel, ISettingsService settingsService)
        {
            _settingsService = Guard.NotNull(settingsService, nameof(settingsService));

            BackCommand = new DelegateCommand(() => WebBrowser.CanGoBack, () => WebBrowser.GoBack(), true);
            ForwardCommand = new DelegateCommand(() => WebBrowser.CanGoForward, () => WebBrowser.GoForward(), true);
            ReloadCommand = new DelegateCommand(() => WebBrowser.Reload());

            DataContext = viewModel;
            InitializeComponent();

            WebBrowser.Loaded += async (s, e) => await InitWebView();

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
                settings.BrowserHomeUrl = _homePage?.ToString();
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
            StateChanged += (s, e) =>
            {
                TitleButtons.SetWindowState(WindowState);
                MainBorder.Margin = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
            };

            LocationChanged += (s, e) => _windowMoved = true;

            var hotkeys = _settingsService.GuiSettings.Hotkeys;
            var keyListener = new KeyListener(100);
            keyListener.KeyPressed += keys =>
            {
                var thread = new Thread(new ThreadStart(() =>
                {
                    var modifierKeys = Keyboard.Modifiers;
                    foreach (var hotkey in hotkeys)
                    {
                        if (hotkey.ModifierKeys == modifierKeys && hotkey.Key == keys)
                        {
                            Dispatcher.Invoke(async () => await WebBrowser.ExecuteScriptAsync(hotkey.Script));
                        }
                    }
                }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            };
            keyListener.StartListener();
            Closed += (s, e) => keyListener.StopListener();
        }

        private async Task InitWebView()
        {
            Directory.CreateDirectory(App.BrowserCachePath);
            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: App.BrowserCachePath);
            await WebBrowser.EnsureCoreWebView2Async(env);

            WebBrowser.Source = _homePage;
        }

        public void Show(string homePage)
        {
            _homePage = GetUri(homePage);
            Show();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowser.Source = _homePage;
        }

        private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void WebBrowser_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            IsLoading = true;
        }

        private void WebBrowser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            IsLoading = false;
        }

        private void WebBrowser_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
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

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.LeftCtrl || e.Key == Key.Right) && !e.IsRepeat)
            {
                ViewModel.ForceShowTitle = true;
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ViewModel.ForceShowTitle = false;
            }
        }

        private void UrlOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _windowMoved = false;
                DragMove();
                if (!_windowMoved)
                    UrlTextBox.Focus();
            }
        }

        private void UrlTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UrlOverlay.IsHitTestVisible = false;
        }

        private void UrlTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UrlOverlay.IsHitTestVisible = true;
        }
    }
}
