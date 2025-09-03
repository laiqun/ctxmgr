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

namespace ctxmgr
{
    /// <summary>
    /// Interaction logic for TabTitleEditor.xaml
    /// </summary>
    public partial class TabTitleEditor : Window
    {
        public TabTitleEditor()
        {
            InitializeComponent();
        }
        private void SetupButtons()
        {
            OKButton.Click += (s, e) => { _result = MessageTextBox.Text; Close(); };
            CancelButton.Click += (s, e) => { _result = String.Empty; Close(); };

        }
        private string _result = string.Empty;
        public static string Show(string? title, string caption = "",
             Window owner = null)
        {
            var msgBox = new TabTitleEditor();
            msgBox.Owner = owner;
            msgBox.TitleTextBlock.Text = caption;
            msgBox.MessageTextBox.Text = title;
            msgBox.MessageTextBox.Focus();
            msgBox.MessageTextBox.SelectAll();
            msgBox.SetupButtons();
            msgBox.ShowDialog();
            return msgBox._result ;
        }

    }
}
