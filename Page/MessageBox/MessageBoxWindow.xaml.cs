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

namespace ctxmgr.Page.MessageBox
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow()
        {
            InitializeComponent();
            PreviewKeyDown += (s, e) =>
            {
                e.Handled = true;
                if (e.Key == Key.Escape) Close();
            };
        }
        private MessageBoxResult _result;
        public static MessageBoxResult Show(string message, string caption = "",
            MessageBoxButton buttons = MessageBoxButton.OK, Window owner = null)
        {
            var msgBox = new MessageBoxWindow();
            msgBox.Owner = owner;
            msgBox.TitleTextBlock.Text = caption;
            msgBox.MessageTextBlock.Text = message;

            // 设置按钮可见性
            msgBox.SetupButtons(buttons);

            msgBox.ShowDialog();
            return msgBox._result;
        }

        private void SetupButtons(MessageBoxButton buttons)
        {
            // 隐藏所有按钮
            YesButton.Visibility = Visibility.Collapsed;
            NoButton.Visibility = Visibility.Collapsed;
            OKButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;

            switch (buttons)
            {
                case MessageBoxButton.OK:
                    OKButton.Visibility = Visibility.Visible;
                    OKButton.Click += (s, e) => { _result = MessageBoxResult.OK; Close(); };
                    break;

                case MessageBoxButton.OKCancel:
                    OKButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    OKButton.Click += (s, e) => { _result = MessageBoxResult.OK; Close(); };
                    CancelButton.Click += (s, e) => { _result = MessageBoxResult.Cancel; Close(); };
                    break;

                case MessageBoxButton.YesNo:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    YesButton.Click += (s, e) => { _result = MessageBoxResult.Yes; Close(); };
                    NoButton.Click += (s, e) => { _result = MessageBoxResult.No; Close(); };
                    break;

                case MessageBoxButton.YesNoCancel:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    YesButton.Click += (s, e) => { _result = MessageBoxResult.Yes; Close(); };
                    NoButton.Click += (s, e) => { _result = MessageBoxResult.No; Close(); };
                    CancelButton.Click += (s, e) => { _result = MessageBoxResult.Cancel; Close(); };
                    break;
            }
        }
    }
    public static class MyMessageBox
    {
        public static MessageBoxResult Show(string messageBoxText)
        {
            return MessageBoxWindow.Show(messageBoxText);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            return MessageBoxWindow.Show(messageBoxText, caption);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            return MessageBoxWindow.Show(messageBoxText, caption, button);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, Window owner)
        {
            return MessageBoxWindow.Show(messageBoxText, caption, button, owner);
        }
    }
}
