using ctxmgr.Page.Settings;
using ctxmgr.Properties;
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
            PreviewKeyDown += (s, e) =>
            {
                e.Handled = true;
                if (e.Key == Key.Escape) Close();
            };
            this.DataContext = ctxmgr.Properties.Config.ConfigInstance.Style;

            var allFonts = Fonts.SystemFontFamilies.ToList();
            //allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code Light"));
            allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code"));
            //allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code Medium"));
            //allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code SemiBold"));
            //allFonts.Add(new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#Google Sans Code ExtraBold"));
            FontsListBox.ItemsSource = allFonts;
            if (FontsListBox.SelectedItem != null)
                FontsListBox.ScrollIntoView(FontsListBox.SelectedItem);
            this.Loaded += FontSettingsWindow_Loaded;
        }

        private void FontSettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. 获取当前的绑定值，例如从DataContext中获取ViewModel，再获取FontFamily
            var viewModel = (StyleSetting)this.DataContext; // 假设DataContext就是你的FontSetting实例
            string targetFontFamily = viewModel.FontFamily;

            // 2. 在ListBox的项中查找与目标值匹配的项
            foreach (var item in FontsListBox.Items)
            {
                // 假设你的项有一个名为"Source"的属性，并且你将其作为DisplayMemberPath和SelectedValuePath
                // 你需要根据你实际的数据项对象结构来获取用于比较的值
                // 这里是一个示例，如果你的项是动态对象或具有特定属性，请调整代码
                //if (item is YourItemType yourItem && yourItem.Source == targetFontFamily)
                //{
                // 3. 找到后，调用ScrollIntoView滚动到该项
                //  FontsListBox.ScrollIntoView(item);
                // break; // 找到后退出循环
                //}
                // 或者，如果你ItemsSource中的项本身就是字符串（即Source的值），可以直接比较
                if (item.ToString() == targetFontFamily)
                {
                    FontsListBox.ScrollIntoView(item);
                    break;
                }
            }
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
