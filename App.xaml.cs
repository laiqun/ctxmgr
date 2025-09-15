using Hardcodet.Wpf.TaskbarNotification;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;


namespace ctxmgr
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string UniqueMutexName = "ctxmgr.com.ctxmgr.UniqueStartupMutex";
        private static Mutex _mutex;
        public static TaskbarIcon mTaskbarIcon;
        public static bool IsDuplicateInstance = false;
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, UniqueMutexName, out createdNew);
            ctxmgr.Properties.Config.ConfigInstance = ctxmgr.Properties.Config.Load();
            if (!createdNew)
            {
                // 已存在运行实例
                ActivateExistingWindow();
                IsDuplicateInstance = true;
                Shutdown();
                return;
            }
            CultureInfo current = Thread.CurrentThread.CurrentUICulture;
            if (current.TwoLetterISOLanguageName == "zh")
                ctxmgr.Properties.Resources.Culture = new CultureInfo("zh");
            mTaskbarIcon = (TaskbarIcon)FindResource("Taskbar");
            mTaskbarIcon.DataContext = new TaskbarIconViewModel();
            //mTaskbarIcon.MenuActivation = PopupActivationMode.LeftOrRightClick;
            // 创建 ContextMenu
            var menu = new ContextMenu() { StaysOpen = false };

            // 创建 MenuItem
            menu.Items.Add(new MenuItem
            {
                Header = ctxmgr.Properties.Resources.ShowWindow,
                Command = ((TaskbarIconViewModel)mTaskbarIcon.DataContext).Button_ClickCommand,
                CommandParameter = 1
            });

            menu.Items.Add(new MenuItem
            {
                Header = ctxmgr.Properties.Resources.HideWindow2,
                Command = ((TaskbarIconViewModel)mTaskbarIcon.DataContext).Button_ClickCommand,
                CommandParameter = 0
            });

            menu.Items.Add(new Separator());

            menu.Items.Add(new MenuItem
            {
                Header = ctxmgr.Properties.Resources.Exit,
                Command = ((TaskbarIconViewModel)mTaskbarIcon.DataContext).Button_ClickCommand,
                CommandParameter = 99
            });

            // 绑定到 TaskbarIcon
            mTaskbarIcon.ContextMenu = menu;

            mTaskbarIcon.TrayLeftMouseDown += MTaskbarIcon_TrayLeftMouseDown;
            base.OnStartup(e);

        }

        private void MTaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            ctxmgr.MainWindow.Instance.Show();
            if (ctxmgr.MainWindow.Instance.WindowState == WindowState.Minimized)
            {
                ctxmgr.MainWindow.Instance.WindowState = WindowState.Normal;
            }
            ctxmgr.MainWindow.Instance.Activate();
            ctxmgr.MainWindow.Instance.Topmost = true;
            if (!ctxmgr.Properties.Config.ConfigInstance.StayOnTop)
                ctxmgr.MainWindow.Instance.Topmost = false;

            ctxmgr.MainWindow.Instance.Focus();

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
            try
            {
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex(); // 只释放自己拥有的
                    _mutex.Dispose();
                }
            }
            catch (ApplicationException)
            {
                // 如果不是拥有者，则忽略
            }
            base.OnExit(e);
        }

        //public void ChangeGlobalFont(string fontResourceKey = "PrimaryFont",string value= "Microsoft YaHei UI")
        //{
        //    // 获取当前应用程序的资源字典
        //    var resources = Application.Current.Resources;

        //    // 根据传入的Key获取新的FontFamily资源
        //    // 注意：确保资源确实存在，否则需要进行检查
        //    var newFontFamily = new FontFamily(value); ;

        //    if (newFontFamily != null)
        //    {
        //        // 直接修改资源字典中对应的全局样式设置器（Setter）的值
        //        // 由于样式是StaticResource，修改后需要手动通知UI更新
        //        // 一种方法是直接替换整个Style，但更简单的方法是修改Setter的值
        //        // 另一种可靠的方法是重新赋值资源本身，并触发资源失效通知
        //        resources["PrimaryFont"] = newFontFamily; // 直接替换PrimaryFont资源

        //        // 强制重新加载所有使用PrimaryFont静态资源的元素（方法较粗暴，但有效）
        //        // 注意：这可能对性能有影响，但对于中小型应用通常可以接受
        //        Resources.MergedDictionaries.Clear();
        //        Resources.MergedDictionaries.Add(Resources);
        //    }
        //    //用法
        //    /*if (Application.Current is App app)
        //    {
        //        string selectedFontKey = ((ComboBoxItem)FontSelectionComboBox.SelectedItem).Tag.ToString();
        //        app.ChangeGlobalFont(selectedFontKey); // 例如: "MonoFont"
        //    }*/
        //}
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
