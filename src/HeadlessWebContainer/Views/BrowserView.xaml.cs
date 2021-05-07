using CefSharp;
using HeadlessWebContainer.Models;
using HeadlessWebContainer.ViewModels;
using MaSch.Core;
using System.Windows;
using System.Windows.Input;

namespace HeadlessWebContainer.Views
{
    public partial class BrowserView
    {
        private readonly string _homePage;

        public BrowserViewModel ViewModel => (BrowserViewModel)DataContext;

        public BrowserView(string homePage)
        {
            _homePage = homePage;

            DataContext = new BrowserViewModel();
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var settings = ServiceContext.GetService<GuiSettings>();
                settings.ApplyToWindow(this);
                ViewModel.IsTitlePinned = settings.IsTitlePinned;
                TitleButtons.SetWindowState(WindowState);
            };
            Closed += (s, e) =>
            {
                var settings = ServiceContext.GetService<GuiSettings>();
                settings.IsTitlePinned = ViewModel.IsTitlePinned;
                settings.UpdateFromWindow(this);
            };
            Activated += (s, e) => ViewModel.ForceShowTitle = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            Deactivated += (s, e) => ViewModel.ForceShowTitle = false;

            TitleButtons.CloseButtonClicked += Close;
            TitleButtons.MinimizeButtonClicked += () => WindowState = WindowState.Minimized;
            TitleButtons.NormalizeButtonClicked += () => WindowState = WindowState.Normal;
            TitleButtons.MaximizeButtonClicked += () => WindowState = WindowState.Maximized;
            StateChanged += (s, e) => TitleButtons.SetWindowState(WindowState);

            WebBrowser.Address = _homePage;
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            WebBrowser.NotifyDpiChange((float)newDpi.DpiScaleX);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5 && !e.IsRepeat)
            {
                WebBrowser.Reload(e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control));
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
            WebBrowser.Address = _homePage;
        }

        private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
