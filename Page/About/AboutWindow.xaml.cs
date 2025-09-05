using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace ctxmgr.Page.About
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            LoadAssemblyInfo();
  
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) Hide();
            };
        }
        private void LoadAssemblyInfo()
        {
            var asm = Assembly.GetExecutingAssembly();
            var name = asm.GetName().Name;
            var version = asm.GetName().Version?.ToString(3) ?? "1.0.0";

            AppNameText.Text = name;
            VersionText.Text = $"版本 {version}";

            // 读取 AssemblyCopyrightAttribute
            var copyrightAttr = asm.GetCustomAttribute<AssemblyCopyrightAttribute>();
            CopyrightText.Text = copyrightAttr?.Copyright ?? $"© {DateTime.Now.Year} {name}";
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
        private void Hyperlink_OnRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
