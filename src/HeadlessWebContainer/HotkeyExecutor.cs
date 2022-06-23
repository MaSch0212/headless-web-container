using HeadlessWebContainer.Models;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace MaSch.Native.Windows.Input;

public sealed class HotkeyExecutor : IDisposable
{
    private readonly HotkeyDefinition _hotkey;
    private readonly WebView2 _browser;
    private readonly SemaphoreSlim _workerLock = new(1);
    private Worker? _worker;

    public HotkeyExecutor(HotkeyDefinition hotkey, WebView2 browser)
    {
        _hotkey = hotkey;
        _browser = browser;
    }

    public async Task StartAsync()
    {
        await _workerLock.WaitAsync();
        try
        {
            if (_worker is not null || _hotkey.Key <= 0 || (int)_hotkey.Key > 254)
                return;

            if (_hotkey.Mode == HotkeyMode.KeyPress && !string.IsNullOrWhiteSpace(_hotkey.KeyPressScript))
            {
                _worker = Worker.StartNew(
                    _hotkey.Key,
                    _hotkey.ModifierKeys,
                    onKeyPress: () => SendScriptToBrowser(x => x.KeyPressScript));
            }
            else if (_hotkey.Mode == HotkeyMode.KeyUpDown && !(string.IsNullOrWhiteSpace(_hotkey.KeyUpScript) && string.IsNullOrWhiteSpace(_hotkey.KeyDownScript)))
            {
                _worker = Worker.StartNew(
                    _hotkey.Key,
                    _hotkey.ModifierKeys,
                    onKeyDown: () => SendScriptToBrowser(x => x.KeyDownScript),
                    onKeyUp: () => SendScriptToBrowser(x => x.KeyUpScript));
            }
        }
        finally
        {
            _workerLock.Release();
        }
    }

    public async Task StopAsync()
    {
        await _workerLock.WaitAsync();
        try
        {
            if (_worker is null)
                return;

            await _worker.DisposeAsync();
            _worker = null;
        }
        finally
        {
            _workerLock.Release();
        }
    }

    public void Dispose()
    {
        StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private void SendScriptToBrowser(Func<HotkeyDefinition, string?> scriptAccessor)
    {
        string? script = scriptAccessor(_hotkey);
        if (string.IsNullOrWhiteSpace(script))
            return;
        _browser.Dispatcher.Invoke(async () => await _browser.ExecuteScriptAsync(script));
    }

    private class Worker : IAsyncDisposable
    {
        private delegate void HotkeyHandler(KeyState oldKeyState, KeyState newKeyState);

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly PeriodicTimer _timer;
        private readonly Task _workerTask;
        private readonly Keys _key;
        private readonly ModifierKeys _modifierKeys;
        private readonly HotkeyHandler _hotkeyHandler;
        private KeyState _lastKeyState;

        private Worker(Keys key, ModifierKeys modifierKeys, HotkeyHandler hotkeyHandler)
        {
            _key = key;
            _modifierKeys = modifierKeys;
            _hotkeyHandler = hotkeyHandler;
            _cancellationTokenSource = new CancellationTokenSource();
            _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));
            _workerTask = DoWork(_cancellationTokenSource.Token);
        }

        public static Worker StartNew(Keys key, ModifierKeys modifierKeys, Action onKeyPress)
            => new(key, modifierKeys, BuildKeyPressHandler(onKeyPress));

        public static Worker StartNew(Keys key, ModifierKeys modifierKeys, Action onKeyDown, Action onKeyUp)
            => new(key, modifierKeys, BuildKeyUpDownHandler(onKeyDown, onKeyUp));

        public async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _workerTask;
            _cancellationTokenSource.Dispose();
            _timer.Dispose();
        }

        private static HotkeyHandler BuildKeyPressHandler(Action onKeyPress)
        {
            return new HotkeyHandler((oldKeyState, newKeyState) =>
            {
                if (newKeyState == (KeyState.IsDown | KeyState.IsToggle))
                    onKeyPress();
            });
        }

        private static HotkeyHandler BuildKeyUpDownHandler(Action onKeyDown, Action onKeyUp)
        {
            return new HotkeyHandler((oldKeyState, newKeyState) =>
            {
                if (oldKeyState.HasFlag(KeyState.IsDown) && !newKeyState.HasFlag(KeyState.IsDown))
                    onKeyUp();
                else if (!oldKeyState.HasFlag(KeyState.IsDown) && newKeyState.HasFlag(KeyState.IsDown))
                    onKeyDown();
            });
        }

        private static bool IsKeyDown(Keys key) => GetAsyncKeyState(key).HasFlag(KeyState.IsDown);

        private async Task DoWork(CancellationToken token)
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(token))
                {
                    KeyState keyState = GetAsyncKeyState(_key);
                    if (_modifierKeys.HasFlag(ModifierKeys.Control) && !(IsKeyDown(Keys.LControlKey) || IsKeyDown(Keys.RControlKey)))
                        keyState = KeyState.None;
                    if (_modifierKeys.HasFlag(ModifierKeys.Shift) && !(IsKeyDown(Keys.LShiftKey) || IsKeyDown(Keys.RShiftKey)))
                        keyState = KeyState.None;
                    if (_modifierKeys.HasFlag(ModifierKeys.Alt) && !(IsKeyDown(Keys.LMenu) || IsKeyDown(Keys.RMenu)))
                        keyState = KeyState.None;
                    if (_modifierKeys.HasFlag(ModifierKeys.Windows) && !(IsKeyDown(Keys.LWin) || IsKeyDown(Keys.RWin)))
                        keyState = KeyState.None;
                    _hotkeyHandler(_lastKeyState, keyState);
                    _lastKeyState = keyState;
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore task cancellation
            }
        }

        [DllImport("User32.dll")]
        public static extern KeyState GetAsyncKeyState(Keys vKey);
    }

    private enum KeyState : ushort
    {
        None = 0,
        IsDown = 0x8000,
        IsToggle = 0x0001,
    }
}
