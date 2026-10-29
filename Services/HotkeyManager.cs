using System;
using System.Windows.Interop;
using WinArma.Utils;

namespace WinArma.Services
{
    public class HotkeyManager : IDisposable
    {
        private const int HOTKEY_ID = 9000;
        private IntPtr _hWnd;
        private HwndSource? _hwndSource;
        private bool _isRegistered = false;

        public event Action? HotkeyPressed;

        public bool Register(IntPtr hWnd, uint modifiers = NativeMethods.MOD_ALT, uint key = NativeMethods.VK_SPACE)
        {
            _hWnd = hWnd;
            _hwndSource = HwndSource.FromHwnd(_hWnd);
            _hwndSource?.AddHook(HwndHook);

            _isRegistered = NativeMethods.RegisterHotKey(_hWnd, HOTKEY_ID, modifiers | NativeMethods.MOD_NOREPEAT, key);
            return _isRegistered;
        }

        public bool UpdateHotkey(string modifierStr, string keyStr)
        {
            if (_hWnd == IntPtr.Zero) return false;

            NativeMethods.UnregisterHotKey(_hWnd, HOTKEY_ID);
            _isRegistered = false;

            uint modifier = modifierStr switch
            {
                "Ctrl" => NativeMethods.MOD_CONTROL,
                "Win" => NativeMethods.MOD_WIN,
                "Alt+Shift" => NativeMethods.MOD_ALT | NativeMethods.MOD_SHIFT,
                "Ctrl+Alt" => NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT,
                _ => NativeMethods.MOD_ALT
            };

            uint vkKey = keyStr switch
            {
                "Space" => NativeMethods.VK_SPACE,
                _ => NativeMethods.VK_SPACE
            };

            _isRegistered = NativeMethods.RegisterHotKey(_hWnd, HOTKEY_ID, modifier | NativeMethods.MOD_NOREPEAT, vkKey);
            if (_isRegistered) return true;

            // Chosen combo is taken by the system (e.g. Win+Space) - keep the launcher reachable
            _isRegistered = NativeMethods.RegisterHotKey(_hWnd, HOTKEY_ID, NativeMethods.MOD_CONTROL | NativeMethods.MOD_NOREPEAT, NativeMethods.VK_SPACE);
            return false;
        }

        public void Unregister()
        {
            if (_isRegistered && _hWnd != IntPtr.Zero)
            {
                NativeMethods.UnregisterHotKey(_hWnd, HOTKEY_ID);
                _hwndSource?.RemoveHook(HwndHook);
                _isRegistered = false;
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke();
                handled = true;
            }

            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
        }
    }
}
