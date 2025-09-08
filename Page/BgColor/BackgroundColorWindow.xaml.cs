using ctxmgr.Page.FontSettings;
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

namespace ctxmgr.Page.BgColor
{
    /// <summary>
    /// Interaction logic for BackgroundColorWindow.xaml
    /// </summary>
    public partial class BackgroundColorWindow : Window
    {
        public BackgroundColorWindow()
        {
            InitializeComponent();
            this.DataContext = new BackgroundColorWindowViewModel();
        }
        public static void Show(
             Window owner)
        {
            var settingWindow = new BackgroundColorWindow();
            settingWindow.Owner = owner;
            settingWindow.ShowDialog();
        }
    }

}
