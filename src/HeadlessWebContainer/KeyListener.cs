﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MaSch.Native.Windows.Input;

public class KeyListener
{
    private readonly bool[] _keyStates = new bool[255];
    private readonly int _interval;
    private Thread? _listener;
    private bool _stopRequested;

    public KeyListener(int checkInterval = 100)
    {
        _interval = checkInterval;
    }

    public event Action<Keys>? KeyPressed;

    public static bool IsControlKey(Keys key)
    {
        return key == Keys.ControlKey || key == Keys.LControlKey || key == Keys.RControlKey;
    }

    public static bool IsShiftKey(Keys key)
    {
        return key == Keys.ShiftKey || key == Keys.LShiftKey || key == Keys.RShiftKey;
    }

    public void StopListener()
    {
        _stopRequested = true;
    }

    public void StartListener()
    {
        if (_listener != null && _listener.IsAlive)
            _listener.Join();

        _listener = new Thread(new ThreadStart(DoListenerAction));
        _listener.SetApartmentState(ApartmentState.STA);
        _listener.Start();
    }

    public bool IsKeyPressed(Keys key)
    {
        return _keyStates[(int)key];
    }

    private void DoListenerAction()
    {
        while (!_stopRequested)
        {
            for (int i = 1; i < 255; i++)
            {
                if (GetAsyncKeyState(i) == -32767)
                {
                    if (!_keyStates[i])
                    {
                        _keyStates[i] = true;
                        KeyPressed?.Invoke((Keys)i);
                    }
                }
                else
                {
                    _keyStates[i] = false;
                }
            }

            Thread.Sleep(_interval);
        }
    }

    [DllImport("User32.dll")]
    public static extern short GetAsyncKeyState(int vKey);
}