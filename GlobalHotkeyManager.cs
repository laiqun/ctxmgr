using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;


namespace ctxmgr
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
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const int HOTKEY_ID_APPEND = 9001;
        private IntPtr _windowHandle;
        private HwndSource _source;

        public event EventHandler HotkeyPressed;
        public event EventHandler<ClipEventArgs> HotkeyAppendPressed;
        
        public void Register(Window window)
        {
            _windowHandle = new WindowInteropHelper(window).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            // 注册 Alt+C 快捷键

            if (!RegisterHotKey(_windowHandle, HOTKEY_ID, (uint)ModifierKeys.Alt, (uint)KeyInterop.VirtualKeyFromKey(Key.C)))
            {
                window.Hide();
                if (MessageBox.Show(ctxmgr.Properties.Resources.AltZRegistrationFailed, ctxmgr.Properties.Resources.OK) == MessageBoxResult.OK)
                    Application.Current.Shutdown();
            }
            if (!RegisterHotKey(_windowHandle, HOTKEY_ID_APPEND, (uint)ModifierKeys.Control, (uint)KeyInterop.VirtualKeyFromKey(Key.Q)))
            {
                window.Hide();
                if (MessageBox.Show(ctxmgr.Properties.Resources.CtrlQRegistrationFailed, ctxmgr.Properties.Resources.OK) == MessageBoxResult.OK)
                    Application.Current.Shutdown();
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg != WM_HOTKEY)
                return IntPtr.Zero;

            if(wParam.ToInt32() == HOTKEY_ID)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            else if (wParam.ToInt32() == HOTKEY_ID_APPEND)
            {
                string oldClipboradText = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
                AppendFunc();
                if (oldClipboradText != string.Empty)
                {
                    //https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid
                    Clipboard.SetDataObject(oldClipboradText);//Clipboard.SetText will raise app crash
                }
                   
            }
            return IntPtr.Zero;
        }
        private IntPtr AppendFunc()
        {
            KeyboardSimulator.SendCtrlC();
            Thread.Sleep(100);
            if (!Clipboard.ContainsText())
                return IntPtr.Zero;
            string clipboradText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboradText))
                return IntPtr.Zero;

            HotkeyAppendPressed?.Invoke(this, new ClipEventArgs(clipboradText));
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _source?.RemoveHook(HwndHook);
            }
        }
    }
}
