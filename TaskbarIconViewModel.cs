using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ctxmgr
{
    public partial class TaskbarIconViewModel: ObservableObject
    {
        [ObservableProperty]
        private string systemTime;
        
        [RelayCommand]
        private void Button_Click(object? type) {
            
            if (type.ToString() == "0")
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            if (type.ToString() == "1")
            {
                if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.Activate();
            }
            if (type.ToString() == "99")
                Application.Current.Shutdown();
        }
        
    }
}
