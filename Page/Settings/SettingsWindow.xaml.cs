using ctxmgr.Page.MessageBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ctxmgr.Page.Settings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            List<string> baseKeys = new List<string>();
            for(int i= 0;i<26;i++)
                baseKeys .Add(((Key)((int)Key.A + i)).ToString());
            HotKeyBase.ItemsSource = baseKeys;
            HotKeyBase.SelectedValue = ((Key)(ctxmgr.Properties.Config.ConfigInstance.HotKeyBase
                )).ToString();
            // 0: None, 1: Alt, 2: Ctrl, 4: Shift, 8: Win
            AltCheckBox.IsChecked = (ctxmgr.Properties.Config.ConfigInstance.HotKeyModifiers & 1) != 0;
            CtrlCheckBox.IsChecked = (ctxmgr.Properties.Config.ConfigInstance.HotKeyModifiers & 2) != 0;
            ShiftCheckBox.IsChecked = (ctxmgr.Properties.Config.ConfigInstance.HotKeyModifiers & 4) != 0;
            WinCheckBox.IsChecked = (ctxmgr.Properties.Config.ConfigInstance.HotKeyModifiers & 8) != 0;
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) { e.Handled = true; Close(); }
            };
        }
 

        public static void Show(
             Window owner,bool isTextSnippets = false)
        {
            var vm = new SettingsViewModel();
            var settingWindow = new SettingsWindow();
            settingWindow.Owner = owner;
            settingWindow.DataContext = vm;
            if(isTextSnippets)
                settingWindow.TextSnippetsTabItem.IsSelected = true;
            settingWindow.ShowDialog();
        }

        private void RestButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as SettingsViewModel;
            if (vm == null) return;
            vm.ResetToDefault();
        }


    }
}
