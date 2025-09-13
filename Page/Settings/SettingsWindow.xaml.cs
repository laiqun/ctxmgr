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
        private bool IsInitDataLoading = true;
        private Func<Key, int,bool> HotKeyChangedFunc;
        public SettingsWindow()
        {
            InitializeComponent();
            IsInitDataLoading = true;
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
            IsInitDataLoading = false;
        }
 

        public static void Show(
             Window owner,bool isTextSnippets = false, Func<Key, int, bool> hotKeyChangedFunc = null)
        {
            var vm = new SettingsViewModel();
            var settingWindow = new SettingsWindow();
            settingWindow.Owner = owner;
            settingWindow.DataContext = vm;
            if(isTextSnippets)
                settingWindow.TextSnippetsTabItem.IsSelected = true;
            settingWindow.HotKeyChangedFunc = hotKeyChangedFunc;
            settingWindow.ShowDialog();
        }

        private void RestButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as SettingsViewModel;
            if (vm == null) return;
            vm.ResetToDefault();
        }
        private void HotKeyChanged()
        {
            if (IsInitDataLoading) return;
            var baseKeyOffset = HotKeyBase.SelectedIndex;
            if(baseKeyOffset < 0 || baseKeyOffset > 25) return; 
            var baseKey = (Key)((int)Key.A + baseKeyOffset);
            int modifiers = 0;
            if (AltCheckBox.IsChecked == true) modifiers |= 1;
            if (CtrlCheckBox.IsChecked == true) modifiers |= 2;
            if (ShiftCheckBox.IsChecked == true) modifiers |= 4;
            if (WinCheckBox.IsChecked == true) modifiers |= 8;
            var rst = HotKeyChangedFunc?.Invoke(baseKey, modifiers);
            if (rst == false)
            {
                HotKeyChangedFunc(Key.Z,(int)ModifierKeys.Alt);
                TxtStatus.Text = "修改失败，恢复为原始快捷键";
            }
            else
            {
                TxtStatus.Text = "修改成功";
            }
        }
        private void HotKeyBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HotKeyChanged();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HotKeyChanged();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            HotKeyChanged();
        }

    }
}
