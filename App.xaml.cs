using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace ctxmgr
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string UniqueMutexName = "ctxmgr.com.ctxmgr.UniqueStartupMutex";
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, UniqueMutexName, out createdNew);

            if (!createdNew)
            {
                // 已存在运行实例
                ActivateExistingWindow();
                Shutdown();
                return;
            }
            CultureInfo current = Thread.CurrentThread.CurrentUICulture;
            if(current.TwoLetterISOLanguageName == "zh")
                ctxmgr.Properties.Resources.Culture = new CultureInfo("zh");
            base.OnStartup(e);
        }
        private void ActivateExistingWindow()
        {
            // 通过进程间通信激活已有实例窗口
            var current = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id == current.Id) continue;

                NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                NativeMethods.ShowWindow(process.MainWindowHandle, NativeMethods.SW_RESTORE);
                break;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const int SW_RESTORE = 9;
    }
}
