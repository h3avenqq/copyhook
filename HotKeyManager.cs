using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace KeyDownTester.Keys
{
    public static class HotkeysManager
    {

        public delegate void HotkeyEvent(GlobalHotkey hotkey);

        public static event HotkeyEvent HotkeyFired;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc LowLevelProc = HookCallback;

        private static List<GlobalHotkey> Hotkeys { get; set; }


        private const int WH_KEYBOARD_LL = 13;

        private static IntPtr HookID = IntPtr.Zero;

        public static bool IsHookSetup { get; private set; }


        public static bool RequiresModifierKey { get; set; }

        static HotkeysManager()
        {
            Hotkeys = new List<GlobalHotkey>();
            RequiresModifierKey = true;
        }

        public static void SetupSystemHook()
        {
            HookID = SetHook(LowLevelProc);
            IsHookSetup = true;
        }

        public static void ShutdownSystemHook()
        {
            UnhookWindowsHookEx(HookID);
            IsHookSetup = false;
        }

        public static void AddHotkey(GlobalHotkey hotkey)
        {
            Hotkeys.Add(hotkey);
        }

        public static void RemoveHotkey(GlobalHotkey hotkey)
        {
            Hotkeys.Remove(hotkey);
        }

        private static void CheckHotkeys()
        {
            if (RequiresModifierKey)
            {
                if (Keyboard.Modifiers != ModifierKeys.None)
                {
                    foreach (GlobalHotkey hotkey in Hotkeys)
                    {
                        if (Keyboard.Modifiers == hotkey.Modifier && Keyboard.IsKeyDown(hotkey.Key))
                        {
                            if (hotkey.CanExecute)
                            {
                                hotkey.Callback?.Invoke();
                                HotkeyFired?.Invoke(hotkey);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (GlobalHotkey hotkey in Hotkeys)
                {
                    if (Keyboard.Modifiers == hotkey.Modifier && Keyboard.IsKeyDown(hotkey.Key))
                    {
                        if (hotkey.CanExecute)
                        {
                            hotkey.Callback?.Invoke();
                            HotkeyFired?.Invoke(hotkey);
                        }
                    }
                }
            }
        }


        public static void AddHotkey(ModifierKeys modifier, Key key, Action callbackMethod, bool canExecute = true)
        {
            AddHotkey(new GlobalHotkey(modifier, key, callbackMethod, canExecute));
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                CheckHotkeys();
            }
            return CallNextHookEx(HookID, nCode, wParam, lParam);
        }

        #region Native Methods

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}