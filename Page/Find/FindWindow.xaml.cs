using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ctxmgr.Page.Find
{
    /// <summary>
    /// Interaction logic for FindWindow.xaml
    /// </summary>
    public partial class FindWindow : Window
    {
        public string Keyword => TxtKeyword.Text;
        public bool MatchCase => ChkCaseSensitive.IsChecked == true;
        public bool WholeWord => ChkWholeWord.IsChecked == true;
        public string ReplaceTxt => TxtReplace.Text;

        public event Func<string, bool, bool, bool,bool>? FindRequested;
        // 参数：keyword, forward, matchCase, wholeWord
        public event Func<string, bool, bool, bool,string, bool>? ReplaceRequested;
        
        public event Action<string, string, bool, bool>? ReplacAllRequested;
        // 参数：keyword, replaceText, matchCase, wholeWord
        public FindWindow(string keyWord)
        {
            InitializeComponent();
            if(!string.IsNullOrEmpty(keyWord))
            {
                TxtKeyword.Text = keyWord;
                TxtKeyword.SelectAll();
            }
            else
            {
                TxtKeyword.Focus();
            }
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) { e.Handled = true; Close(); }
            };
        }
        private FindWindow()
        {
            InitializeComponent();
        }
        private void BtnFindNext_Click(object sender, RoutedEventArgs e)
        {
           bool? rst = FindRequested?.Invoke(Keyword, chkBackward.IsChecked == false, MatchCase, WholeWord);
           TxtStatus.Text = rst == false? ctxmgr.Properties.Resources.NotFound +$": \"{Keyword}\"":"";
        }

        private void BtnReplace_Click(object sender, RoutedEventArgs e)
        {
            var rst = ReplaceRequested?.Invoke(Keyword, chkBackward.IsChecked == false, MatchCase, WholeWord, ReplaceTxt);
            TxtStatus.Text = rst == false ? ctxmgr.Properties.Resources.NoMoreFound + $": \"{Keyword}\"" : "";
        }

        private void BtnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            ReplacAllRequested?.Invoke(Keyword, ReplaceTxt, MatchCase, WholeWord);
        }
    }
}
