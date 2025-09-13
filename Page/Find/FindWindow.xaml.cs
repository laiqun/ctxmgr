using System;
using System.Collections.Generic;
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

        public event Action<string, bool, bool, bool>? FindRequested;
        // 参数：keyword, forward, matchCase, wholeWord
        public FindWindow()
        {
            InitializeComponent();
        }
        private void BtnFindNext_Click(object sender, RoutedEventArgs e)
        {
            FindRequested?.Invoke(Keyword, true, MatchCase, WholeWord);
        }

        private void BtnFindPrev_Click(object sender, RoutedEventArgs e)
        {
            FindRequested?.Invoke(Keyword, false, MatchCase, WholeWord);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
