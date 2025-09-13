using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;



namespace ctxmgr.Utilities
{
    public class ClipEventArgs : EventArgs
    {
        public string Message { get; }
        public ClipEventArgs(string message)
        {
            Message = message;
        }
    }
    public class GlobalHotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(nint hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const int HOTKEY_ID_APPEND = 9001;
        private nint _windowHandle;
        private HwndSource _source;

        public event EventHandler HotkeyPressed;
        public event EventHandler<ClipEventArgs> HotkeyAppendPressed;
        
        public void Register(Window window)
        {
            _windowHandle = new WindowInteropHelper(window).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
            var rst = NativeClipboardMethods.AddClipboardFormatListener(_windowHandle);
            // 注册 Alt+C 快捷键

            if (!RegisterHotKey(_windowHandle, HOTKEY_ID, (uint)ModifierKeys.Alt, (uint)KeyInterop.VirtualKeyFromKey(Key.Z)))
            {
                window.Hide();
                if (MessageBox.Show(Properties.Resources.AltZRegistrationFailed, Properties.Resources.OK) == MessageBoxResult.OK)
                    Application.Current.Shutdown();
            }
            if (!RegisterHotKey(_windowHandle, HOTKEY_ID_APPEND, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.Q)))
            {
                window.Hide();
                if (MessageBox.Show(Properties.Resources.CtrlQRegistrationFailed, Properties.Resources.OK) == MessageBoxResult.OK)
                    Application.Current.Shutdown();
            }
        }
        private string oldClipboardText = string.Empty;
        private bool isExpectingClipboardUpdate = false;

        private nint HwndHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == NativeClipboardMethods.WM_CLIPBOARDUPDATE && isExpectingClipboardUpdate)
            {
                isExpectingClipboardUpdate = false;

                // 异步读取剪贴板并触发事件
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        string newText = SafeGetClipboardText();
                        if (string.IsNullOrEmpty(newText) || newText == previousClipboardText)
                            return;

                        previousClipboardText = newText;

                        HotkeyAppendPressed?.Invoke(this, new ClipEventArgs(newText));

                        // 恢复旧剪贴板
                        if (!string.IsNullOrEmpty(oldClipboardText))
                        {
                            SafeSetClipboard(oldClipboardText);
                        }
                    }
                    catch (COMException)
                    {
                        // 这里可以重试或者忽略
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);

                handled = true;
                return nint.Zero;
            }
            const int WM_HOTKEY = 0x0312;
            if (msg != WM_HOTKEY)
                return nint.Zero;

            if(wParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            else if (wParam.ToInt32() == HOTKEY_ID_APPEND)
            {
                AppendFunc();                   
            }
            
            return nint.Zero;
        }
        private string SafeGetClipboardText(int retryCount = 5, int delay = 50)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    if (Clipboard.ContainsText())
                        return Clipboard.GetText();
                    return string.Empty;
                }
                catch (COMException)
                {
                    Thread.Sleep(delay);
                }
            }
            return string.Empty;
        }
        private void SafeSetClipboard(object data, int retryCount = 5, int delay = 100)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    //https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid
                    //Clipboard.SetText will raise app crash
                    Clipboard.SetDataObject(data, true); // true 表示复制到系统剪贴板
                    return;
                }
                catch (COMException)
                {
                    Thread.Sleep(delay);
                }
            }
        }

        private string previousClipboardText = string.Empty;
        private void AppendFunc()
        {
            // 保存旧剪贴板
            oldClipboardText = SafeGetClipboardText();

            // 标记：我们正在期待下一次剪贴板更新
            isExpectingClipboardUpdate = true;
            KeyboardSimulator.SendCtrlC();
        }

        public void Dispose()
        {
            if (_windowHandle != nint.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                NativeClipboardMethods.RemoveClipboardFormatListener(_windowHandle);
                _source?.RemoveHook(HwndHook);
            }
        }
    }

    internal static class NativeClipboardMethods
    {
        public const int WM_CLIPBOARDUPDATE = 0x031D;
        private static readonly nint HWND_MESSAGE = new nint(-3);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool AddClipboardFormatListener(nint hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RemoveClipboardFormatListener(nint hwnd);
    }

}
