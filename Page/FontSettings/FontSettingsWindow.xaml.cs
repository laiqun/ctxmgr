using ctxmgr.Page.Settings;
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

namespace ctxmgr.Page.FontSettings
{
    /// <summary>
    /// Interaction logic for FontSettingsWindow.xaml
    /// </summary>
    public partial class FontSettingsWindow : Window
    {
        public FontSettingsWindow()
        {
            InitializeComponent();
            this.DataContext = ctxmgr.Properties.Config.ConfigInstance.Font;

            var allFonts = Fonts.SystemFontFamilies.ToList();
            //allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code Light"));
            allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code"));
            //allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code Medium"));
            //allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code SemiBold"));
            //allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code ExtraBold"));
            FontsListBox.ItemsSource = allFonts;
            if (FontsListBox.SelectedItem != null)
                FontsListBox.ScrollIntoView(FontsListBox.SelectedItem);
        }
        public static void Show(
             Window owner)
        {
            var settingWindow = new FontSettingsWindow();
            settingWindow.Owner = owner;
            settingWindow.ShowDialog();
        }
    }
}
