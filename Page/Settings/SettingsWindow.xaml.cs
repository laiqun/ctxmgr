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
        }
 

        public static void Show(
             Window owner)
        {
            var vm = new SettingsViewModel();
            var settingWindow = new SettingsWindow();
            settingWindow.Owner = owner;
            settingWindow.DataContext = vm;
            settingWindow.ShowDialog();
        }
    }
}
