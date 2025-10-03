using AutoUpdaterDotNET;
using CommunityToolkit.Mvvm.Input;
using ctxmgr.Model;
using ctxmgr.Page.BgColor;
using ctxmgr.Page.ChangeTitle;
using ctxmgr.Page.Find;
using ctxmgr.Page.FontSettings;
using ctxmgr.Page.MessageBox;
using ctxmgr.Page.Settings;
using ctxmgr.Page.Toast;
using ctxmgr.Utilities;
using IWshRuntimeLibrary;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using SQLite;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Windows.Threading;






namespace ctxmgr
{
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DatabaseService service = new ctxmgr.Model.DatabaseService();
        SQLite.SQLiteAsyncConnection db = null!;
        private GlobalHotkeyManager _hotkeyManager;
        public static MainWindow Instance { get; private set; }
       
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            this.Deactivated += MainWindow_Deactivated;
            this.Activated += MainWindow_Activated;
            this.ToggleTopmost.IsChecked = this.Topmost;
            // 恢复窗口位置 

            this.DataContext = ctxmgr.Properties.Config.ConfigInstance.Style;
            this.InsertMenus.DataContext = null;
            DynamicMenusContainer.Collection = ctxmgr.Properties.Config.ConfigInstance.CustomerTextSnippets;
            
            this.Topmost = ctxmgr.Properties.Config.ConfigInstance.StayOnTop;
            this.ToggleTopmost.IsChecked = ctxmgr.Properties.Config.ConfigInstance.StayOnTop;


            #region textwrap
            TextWrapMenuItem.IsChecked = ctxmgr.Properties.Config.ConfigInstance.TextWrap;
            //DefaultTextBox.TextWrapping = ctxmgr.Properties.Config.ConfigInstance.TextWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
            foreach (var item in MyTabControl.Items)
            {
                if (item is TabItem tab && tab.Content is TextBox tb)
                {
                    tb.TextWrapping = TextWrapMenuItem.IsChecked ? TextWrapping.Wrap : TextWrapping.NoWrap;
                }
            }
            #endregion
            if (ctxmgr.Properties.Config.ConfigInstance.WindowLeft >= 0 && ctxmgr.Properties.Config.ConfigInstance.WindowTop >= 0)
            {
                // 虚拟桌面范围（所有显示器合并后的区域）
                double left = ctxmgr.Properties.Config.ConfigInstance.WindowLeft;
                double top = ctxmgr.Properties.Config.ConfigInstance.WindowTop;
                double right = left + this.Width;
                double bottom = top + this.Height;
                double width = ctxmgr.Properties.Config.ConfigInstance.WindowWidth > 0 ? ctxmgr.Properties.Config.ConfigInstance.WindowWidth : this.Width;
                double height = ctxmgr.Properties.Config.ConfigInstance.WindowHeight > 0 ? ctxmgr.Properties.Config.ConfigInstance.WindowHeight : this.Height;


                double vLeft = SystemParameters.VirtualScreenLeft;
                double vTop = SystemParameters.VirtualScreenTop;
                //double vRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
                //double vBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

                // 判断窗口是否在虚拟桌面范围内
                if (left >= vLeft && top >= vTop)//&& right <= vRight && bottom <= vBottom
                {
                    this.Left = left;
                    this.Top = top;
                    this.Width = width;
                    this.Height = height;
                }
                else
                {
                    // 不在范围内则回到默认位置
                    this.Left = 100;
                    this.Top = 100;
                }
            }
            //DataObject.AddPastingHandler(DefaultTextBox, OnPasting);

            _hotkeyManager = new GlobalHotkeyManager();
            _hotkeyManager.HotkeyPressed += OnHotkeyPressed;
            _hotkeyManager.HotkeyAppendPressed += HotkeyManager_HotkeyAppendPressed; ;
            Loaded += (s, e) =>
            {
                _hotkeyManager.Register(this);
            };
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromDays(1) };
            timer.Tick += delegate
            {
                AutoUpdater.Start("https://rbsoft.org/updates/AutoUpdaterTestWPF.xml");
            };
            timer.Start();
            System.Windows.Application.Current.Exit += (s, e) => _hotkeyManager.Dispose();
            var dbInitTask = service.OpenOrCreateDatabase(DataFile);
            dbInitTask.Wait();
            db = dbInitTask.Result;
            service.CreateTables(db).Wait();
            LoadTabsFromDatabase(db);
        }
        private bool IsActiveState = true;
        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            IsActiveState = true;
        }

        private void MainWindow_Deactivated(object? sender, EventArgs e)
        {
            if(isLoadingTabs)
                return;
            IsActiveState = false;
            if(isExiting)
                SaveTabToDatabase(MyTabControl.SelectedItem,true);
            else
                SaveTabToDatabase(MyTabControl.SelectedItem,false);
        }

        private void HotkeyManager_HotkeyAppendPressed(object? sender, ClipEventArgs e)
        {
            var tabItem = MyTabControl.SelectedItem as TabItem;
            if(tabItem == null)
                return;
            var textBox = tabItem.Content as TextBox;
            if (textBox == null)
                return;
            textBox.AppendText(e.Message);
            textBox.ScrollToEnd();
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            if (this.Visibility != Visibility.Hidden && 
                WindowState != WindowState.Minimized &&
                (IsActiveState||(!IsActiveState && this.Topmost)))
            {
                this.WindowState = WindowState.Minimized;
                return;
            }

            this.Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();
            Topmost = true;
            if(!ctxmgr.Properties.Config.ConfigInstance.StayOnTop)
                Topmost = false;
            Focus();
        }

        #region Esc hide window
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Escape)
            {
                this.WindowState = WindowState.Minimized;
            }//find actions
            else if ((e.Key == Key.F || e.Key == Key.R) && Keyboard.Modifiers == ModifierKeys.Control)
            {
                FindMenuItem_Click(FindMenuItem, new RoutedEventArgs());
            }
            else if(e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                ToggleSelector_Click(ToggleSelector, new RoutedEventArgs());
            }
            else if (e.Key == Key.F3)
            {
                FindNextMenuItem_Click(FindNextMenuItem, new RoutedEventArgs());
            }//page actions
            else if ((e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control) || e.Key == Key.F4)
            {
                NewTab_Click(NewTab, new RoutedEventArgs());
            }
            else if (e.Key == Key.System && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (Keyboard.IsKeyDown(Key.Delete))
                    DelTab_Click(DelTab, new RoutedEventArgs());
                else if (Keyboard.IsKeyDown(Key.D))
                {
                    TabList.IsSubmenuOpen = true;
                }
                //to first page
                else if (Keyboard.IsKeyDown(Key.Home))
                {
                    if (MyTabControl.Items.Count < 2)
                        return;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MyTabControl.SelectedIndex = 1;
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                //to end page
                else if (Keyboard.IsKeyDown(Key.End))
                {
                    if (MyTabControl.Items.Count < 2)
                        return;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MyTabControl.SelectedIndex = MyTabControl.Items.Count - 1;
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                else
                {
                    for (var curKey = Key.NumPad0; curKey <= Key.NumPad9; curKey++)
                    {
                        if (Keyboard.IsKeyDown(curKey))
                        {
                            var number = curKey - Key.NumPad0;
                            if (number <= 0)
                                number = 10;
                            if (MyTabControl.Items.Count <= number)
                                return;
                            MyTabControl.SelectedIndex = number;
                            break;
                        }
                    }
                    for (var curKey = Key.D0; curKey <= Key.D9; curKey++)
                    {
                        if (Keyboard.IsKeyDown(curKey))
                        {
                            var number = curKey - Key.D0;
                            if (number <= 0)
                                number = 10;
                            if (MyTabControl.Items.Count <= number)
                                return;
                            MyTabControl.SelectedIndex = number;
                            break;
                        }
                    }
                }
            }
            else if (e.Key == Key.F2)
            {
                ChangeTitle_Click(ChangeTitle, new RoutedEventArgs());
            }//insert actions
            else if ((e.Key == Key.OemMinus && Keyboard.Modifiers == ModifierKeys.Control))
            {
                InsertSeparatorMenuItem_Click(InsertSeparatorMenuItem, new RoutedEventArgs());
            }
            else if ((e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control))
            {
                InsertDateTimeMenuItem_Click(InsertDateTimeMenuItem, new RoutedEventArgs());
            }
            else if ((e.Key == Key.OemMinus && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)))
            {
                InsertDateTimeSeparator_Click(InsertDateTimeSeparator, new RoutedEventArgs());
            }
            else if (e.Key == Key.F6)//topmost
            {
                ToggleTopmost_Click(ToggleTopmost, new RoutedEventArgs());
            }
            else if (e.Key == Key.F8)//settings
            {
                SettingsMenuItem_Click(SettingsMenuItem, new RoutedEventArgs());
            }
            else if (e.Key == Key.W && Keyboard.Modifiers == ModifierKeys.Control)//text wrap
            {
                TextWrapMenuItem_Click(TextWrapMenuItem, new RoutedEventArgs());
            }
            //to next page
            else if ((e.Key == Key.Right && Keyboard.Modifiers == ModifierKeys.Alt) ||
                (e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.Control))
            {
                if (MyTabControl.Items.Count <= 2)
                    return;
                int currentIndex = MyTabControl.SelectedIndex;
                int nextIndex = (currentIndex + 1) % MyTabControl.Items.Count;
                if (nextIndex == 0)//skip first menu tab
                    nextIndex = 1;
                //原因：在窗口加载或 TabItem 还没生成时，直接设置 SelectedIndex 可能被 WPF 修正。

                //方法：用 Dispatcher.BeginInvoke 延迟到布局完成后设置：
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MyTabControl.SelectedIndex = nextIndex;
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            //to previous page
            else if ((e.Key == Key.Left && Keyboard.Modifiers == ModifierKeys.Alt) ||
                (e.Key == Key.Tab && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)))
            {
                if (MyTabControl.Items.Count <= 2)
                    return;
                int currentIndex = MyTabControl.SelectedIndex;
                int previousIndex = (currentIndex - 1 + MyTabControl.Items.Count) % MyTabControl.Items.Count;
                if (previousIndex == 0)//skip first menu tab
                    previousIndex = MyTabControl.Items.Count - 1;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MyTabControl.SelectedIndex = previousIndex;
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            else if (Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Shift))
            {
                //move page to left
                if (Keyboard.IsKeyDown(Key.Left))
                {
                    MoveLeftMenuItem_Click(MoveLeftMenuItem, new RoutedEventArgs());
                }
                else if (Keyboard.IsKeyDown(Key.Right))
                {
                    MoveRightMenuItem_Click(MoveRightMenuItem, new RoutedEventArgs());
                }
                //move to the first
                else if (Keyboard.IsKeyDown(Key.Home))
                {
                    MoveLeftMostMenuItem_Click(MoveLeftMostMenuItem, new RoutedEventArgs());
                }
                //move to the end
                else if (Keyboard.IsKeyDown(Key.End))
                {
                    MoveRightMostMenuItem_Click(MoveRightMostMenuItem, new RoutedEventArgs());

                }
            }

        }
        #endregion
        #region paste is file or folder?
        private string AnnotatePathType(string path)
        {
            var trimmedPath = path.Trim('"');

            if (Directory.Exists(trimmedPath))
            {
                return $"[x]@Folder {trimmedPath}\n";
            }
            else if (System.IO.File.Exists(trimmedPath))
            {
                return $"[x]@File {trimmedPath}\n";
            }

            if (Directory.Exists(path))
            {
                return $"[x]@Folder {path}\n";
            }
            else if (System.IO.File.Exists(path))
            {
                return $"[x]@File {path}\n";
            }
            else
            {
                // 无效路径直接返回原始文本（或自定义处理）
                return path;
            }
        }
        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(DataFormats.UnicodeText)) return;

            string raw = (string)e.DataObject.GetData(DataFormats.UnicodeText);
            string transformed = AnnotatePathType(raw);

            // 取消系统默认粘贴
            e.CancelCommand();

            // 手动插入
            var tb = (TextBox)sender;
            int caret = tb.CaretIndex;
            tb.Text = tb.Text.Insert(caret, transformed);
            tb.CaretIndex = caret + transformed.Length;
        }
        #endregion
        
        private readonly struct TabData
        {
            public readonly string Uuid;
            public readonly string Id;
            public readonly string Title;
            public readonly string Content;

            public TabData(string uuid, string id, string title, string content)
            {
                Uuid = uuid;
                Id = id;
                Title = title;
                Content = content;
            }
        }
        


        #region topmost
        private void ToggleTopmost_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
            this.ToggleTopmost.IsChecked = this.Topmost;
            ctxmgr.Properties.Config.ConfigInstance.StayOnTop = this.Topmost;
            ctxmgr.Properties.Config.ConfigInstance.Save();
        }
        #endregion







        #region https://stackoverflow.com/questions/37326546/why-are-my-menus-opening-right-to-left-instead-of-left-to-right
        private static readonly FieldInfo _menuDropAlignmentField;
        static MainWindow()
        {

            _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            System.Diagnostics.Debug.Assert(_menuDropAlignmentField != null);

            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

        }
        private static void SystemParameters_StaticPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            EnsureStandardPopupAlignment();
        }

        private static void EnsureStandardPopupAlignment()
        {
            if (SystemParameters.MenuDropAlignment && _menuDropAlignmentField != null)
            {
                _menuDropAlignmentField.SetValue(null, false);
            }
        }


        #endregion

        
        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutDialog = new Page.About.AboutWindow();
            aboutDialog.Owner = this;
            aboutDialog.ShowDialog();
        }

        private void HideWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private bool isExiting = false;
        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            isExiting = true;
            System.Windows.Application.Current.Shutdown();
        }

        private void LightMode_Click(object sender, RoutedEventArgs e)
        {
            ctxmgr.Properties.Config.ConfigInstance.Style.FontColor = 0xff000000;

            ctxmgr.Properties.Config.ConfigInstance.Style.BackgroundColor = 0xffffffff;

            ctxmgr.Properties.Config.ConfigInstance.Save();
        }

        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            ctxmgr.Properties.Config.ConfigInstance.Style.FontColor = 0xffffffff;

            ctxmgr.Properties.Config.ConfigInstance.Style.BackgroundColor = 0xff000000;
            ctxmgr.Properties.Config.ConfigInstance.Save();
        }

        //loading path AppData\Local\ctxmgr\ctxmgr_Url_qkyp43qq1dqf12ub3mpsvwt1inkryzoa\1.0.0.0
        //%APPDATA%\[公司名]\[程序名]_Url_[随机字符串]\[版本号]\user.config

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (ctxmgr.App.IsDuplicateInstance)
                return;
            SaveState();
            //SaveTabToDatabase(MyTabControl.SelectedItem);  lost focus 会触发保存的
            e.Cancel = true;
            this.WindowState = WindowState.Minimized;
        }
        private void SaveState() {
            if (this.Left == double.NaN)
                return;
            ctxmgr.Properties.Config.ConfigInstance.WindowLeft = this.Left;
            ctxmgr.Properties.Config.ConfigInstance.WindowTop = this.Top;
            ctxmgr.Properties.Config.ConfigInstance.WindowWidth = this.Width;
            ctxmgr.Properties.Config.ConfigInstance.WindowHeight = this.Height;
            var selectedTab = MyTabControl.SelectedItem as TabItem;
            if (selectedTab != null)
            {
                ctxmgr.Properties.Config.ConfigInstance.LastPage = selectedTab.Tag?.ToString() ?? "";
                if (selectedTab.Content is TextBox tb)
                {
                    ctxmgr.Properties.Config.ConfigInstance.LastCaretIndex = tb.CaretIndex;
                }
            }
            ctxmgr.Properties.Config.ConfigInstance.Save();
        }
        private void MyTabControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 获取点击位置相对于TabControl
            Point clickPoint = e.GetPosition(MyTabControl);

            // 获取Header区域总宽度
            double headerTotalWidth = 0;
            double maxHeight = 0;
            foreach (TabItem tabItem in MyTabControl.Items)
            {
                var container = MyTabControl.ItemContainerGenerator.ContainerFromItem(tabItem) as TabItem;
                if (container != null)
                {
                    headerTotalWidth += container.ActualWidth;
                    maxHeight = container.ActualHeight > maxHeight ? container.ActualHeight : maxHeight;
                }
            }

            // 检查是否在Header右侧空白区域
            if (clickPoint.X > headerTotalWidth && clickPoint.Y < maxHeight)
            {
                NewTab_Click(sender, e);
            }
        }

        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            var tabItem = CreateNewTabImpl($"Tab {MyTabControl.Items.Count}","");
            
            
            SaveTabToDatabaseAsync(db,tabItem?.Tag?.ToString(),
                MyTabControl.Items?.Count.ToString(), tabItem?.Header?.ToString(),
                "");
            TextBoxHelper.SetPlaceholder((TextBox)tabItem!.Content, ctxmgr.Properties.Resources.TextBoxHint);
            MyTabControl!.Items!.Add(tabItem);
            MyTabControl.SelectedItem = tabItem;
            SyncTabsToMenu();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingTabs)
                return;
            if (sender is not TextBox tb)
                return;
            if (MyTabControl.SelectedItem is not TabItem selectedTab)
                return;
            TextChangedTabItems[selectedTab] = true;
        }
        private Dictionary<TabItem, bool> TextChangedTabItems = new Dictionary<TabItem, bool>();
        private void DelTab_Click(object sender, RoutedEventArgs e)
        {
            if (MyTabControl.Items.Count <= 2)
                return;
            if (MessageBoxWindow.Show(Properties.Resources.ConfirmDeleteMsg, Properties.Resources.DeleteWindowTitle, MessageBoxButton.YesNo, this) == MessageBoxResult.Yes)
            {
                var oldSelectedIndex = MyTabControl.SelectedIndex;
                if (MyTabControl.SelectedItem != null)
                {
                    var tabItem = MyTabControl.SelectedItem as TabItem;
                    var textBox = tabItem.Content as TextBox;
                    if (textBox != null)
                    {
                        textBox.TextChanged -= TextBox_TextChanged;
                        textBox.LostFocus -= DefaultTextBox_LostFocus;
                        textBox.PreviewKeyDown -= DefaultTextBox_PreviewKeyDown;
                    }
                    TextChangedTabItems.Remove(tabItem);
                    var uuid  = tabItem?.Tag?.ToString();
                    DeleteTabFileAsync(tabItem?.Tag?.ToString());
                    MyTabControl.Items.Remove(MyTabControl.SelectedItem);
                    if (oldSelectedIndex >= MyTabControl.Items.Count)
                    {
                        MyTabControl.SelectedIndex = oldSelectedIndex - 1;
                    }
                    else
                        MyTabControl.SelectedIndex = oldSelectedIndex;

                }
            }
            SyncTabsToMenu();
        }



        private void SyncTabsToMenu()
        {
            //TabList.Items.Clear();
            if (MyTabControl.Items.Count < 2)
                return;
            List<MenuItem> menuItemList = new List<MenuItem>();
            foreach (TabItem tab in MyTabControl.Items)
            {
                if (MyTabControl.Items.IndexOf(tab) == 0)
                    continue;
                var menuItem = new MenuItem
                {
                    Header = tab.Header,
                    IsCheckable = true,
                    IsChecked = tab.IsSelected,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                menuItem.Click += (s, e) =>
                {
                    MyTabControl.SelectedItem = tab; // 点击菜单切换标签页
                };
                menuItemList.Add(menuItem);
            }
            TabList.ItemsSource = menuItemList;
        }

        private void MyTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoadingTabs)
                return;
            SyncTabsToMenu();
            
            #region Get the selected TabItem
            if (MyTabControl.SelectedItem is TabItem selectedTab)
            {
                // Skip focusing if it's the menu tab (first tab)
                if (selectedTab == menuTabItem)
                    return;

                // Get the TextBox from the TabItem's content
                if (selectedTab.Content is TextBox textBox)
                {
                    // Use Dispatcher to ensure UI update happens on UI thread
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                    {
                        textBox.Focus();
                    }));
                }
            }
            #endregion
        }


        private void MainWindowInstance_LocationChanged(object sender, EventArgs e)
        {

        }
        private void UpdatePopupPosition(object sender, EventArgs e)
        {

        }
        private void InsertSeparatorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Get the currently active TextBox
            TabItem selectedTab = MyTabControl.SelectedItem as TabItem;
            if (selectedTab == null) return;

            TextBox activeTextBox = selectedTab.Content as TextBox;
            if (activeTextBox == null) return;

            // Create a separator line (80 dashes)
            string separator = TextSnippetsHelper.ProccessCharsFunc(Properties.Config.ConfigInstance.InsertLineText);

            // Insert the separator at the current caret position
            int caretIndex = activeTextBox.CaretIndex;

            // Add newlines before and after if needed
            string textToInsert = "";
            bool addLeadingNewLine = caretIndex > 0 && !activeTextBox.Text.Substring(caretIndex - 1, 1).Equals("\n");
            bool addTrailingNewLine = caretIndex < activeTextBox.Text.Length && !activeTextBox.Text.Substring(caretIndex, 1).Equals("\n");
            //•	如果光标不是在最开头（caretIndex > 0），并且光标前一个字符不是换行符（\n），就需要在分隔线前加一个换行符。
            if (addLeadingNewLine) textToInsert += Environment.NewLine;
            textToInsert += separator;
            //•	如果光标不是在文本末尾（caretIndex < activeTextBox.Text.Length），并且光标当前位置的字符不是换行符（\n），就需要在分隔线后加一个换行符。
            if (addTrailingNewLine) textToInsert += Environment.NewLine;

            activeTextBox.Text = activeTextBox.Text.Insert(caretIndex, textToInsert);

            // Move caret to the end of inserted separator
            activeTextBox.CaretIndex = caretIndex + textToInsert.Length;
            activeTextBox.Focus();
        }

        private void InsertDateTimeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var currentDateTime = DateTime.Now.ToString(Properties.Config.ConfigInstance.InsertDateText);
            InsertTextAtCursor(currentDateTime);
        }

        private void InsertDateTimeSeparator_Click(object sender, RoutedEventArgs e)
        {
            // Get the currently active TextBox
            TabItem selectedTab = MyTabControl.SelectedItem as TabItem;
            if (selectedTab == null) return;

            TextBox activeTextBox = selectedTab.Content as TextBox;
            if (activeTextBox == null) return;

            var currentDateTime = TextSnippetsHelper.ProcessCharsAndDateTime(Properties.Config.ConfigInstance.InsertLineDateText, Properties.Config.ConfigInstance.InsertDateText);
            var formattedText = currentDateTime;
            // Insert the separator at the current caret position
            int caretIndex = activeTextBox.CaretIndex;

            // Add newlines before and after if needed
            string textToInsert = "";
            bool addLeadingNewLine = caretIndex > 0 && !activeTextBox.Text.Substring(caretIndex - 1, 1).Equals("\n");
            bool addTrailingNewLine = caretIndex < activeTextBox.Text.Length && !activeTextBox.Text.Substring(caretIndex, 1).Equals("\n");
            //•	如果光标不是在最开头（caretIndex > 0），并且光标前一个字符不是换行符（\n），就需要在分隔线前加一个换行符。
            if (addLeadingNewLine) textToInsert += Environment.NewLine;
            textToInsert += formattedText;
            //•	如果光标不是在文本末尾（caretIndex < activeTextBox.Text.Length），并且光标当前位置的字符不是换行符（\n），就需要在分隔线后加一个换行符。
            if (addTrailingNewLine) textToInsert += Environment.NewLine;

            activeTextBox.Text = activeTextBox.Text.Insert(caretIndex, textToInsert);

            // Move caret to the end of inserted separator
            activeTextBox.CaretIndex = caretIndex + textToInsert.Length;
            activeTextBox.Focus();
        }


        private void InsertTextAtCursor(string text)
        {
            if (MyTabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is TextBox textBox)
                {
                    int caretIndex = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Insert(caretIndex, text);
                    textBox.CaretIndex = caretIndex + text.Length;
                    textBox.Focus();
                }
            }
        }


        private void SoftwareDescriptionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 判断当前 UI 文化
            var culture = CultureInfo.CurrentUICulture;
            string fileName = culture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? "Readme_zh.txt"
                : "Readme.txt";

            // 获取 exe 所在目录
            string exeDir = System.AppContext.BaseDirectory;
            string filePath = System.IO.Path.Combine(exeDir, fileName);

            // 用 start 命令打开
            var psi = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c start \"\" \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi);
        }
        
        private void ToggleSelector_Click(object sender, RoutedEventArgs e)
        {
            var curTab = MyTabControl.SelectedItem as TabItem;
            if (curTab == null)
                return;

            var uuid = curTab.Tag.ToString();
            if (string.IsNullOrEmpty(uuid))
                return;
            var page = GetWorkdSpaceAsync(db, uuid);


            var selectorWindow = new Page.FileFolderSelector.FileFolderSelector(SetWorkSpace, page?.Workspace, uuid, SetSelectedList, page?.SelectedListItems);
            selectorWindow.Owner = this;
            selectorWindow.ShowDialog();
            return;
            /*
            this.ToggleRunOnStartUp.IsChecked = !this.ToggleRunOnStartUp.IsChecked;

            AutoStartHelper.SetAutoStart(this.ToggleRunOnStartUp.IsChecked == true);
            ctxmgr.Properties.Config.ConfigInstance.RunOnStartUp = this.ToggleRunOnStartUp.IsChecked == true;
            ctxmgr.Properties.Config.ConfigInstance.Save();*/
        }
        private void SetSelectedList(string selectList)
        {
            var curTab = MyTabControl.SelectedItem as TabItem;
            if (curTab == null)
                return;
            var uuid = curTab.Tag.ToString();
            if (string.IsNullOrEmpty(uuid))
                return;
            UpdateSelectedListAsync(db, uuid, selectList);
        }
        private void SetWorkSpace(string workspace)
        {
            var curTab = MyTabControl.SelectedItem as TabItem;
            if (curTab == null)
                return;
            var uuid = curTab.Tag.ToString();
            if (string.IsNullOrEmpty(uuid))
                return;
            UpdateWorkdSpaceAsync(db, uuid, workspace);
        }
        private void TextWrapMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Toggle word wrap for current tab's TextBox
            if (MyTabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is TextBox textBox)
                {
                    textBox.TextWrapping = TextWrapMenuItem.IsChecked ? TextWrapping.Wrap : TextWrapping.NoWrap;
                }
            }

            // Save preference to config
            ctxmgr.Properties.Config.ConfigInstance.TextWrap = TextWrapMenuItem.IsChecked;
            ctxmgr.Properties.Config.ConfigInstance.Save();

            // Apply to all tabs if desired
            foreach (var item in MyTabControl.Items)
            {
                if (item is TabItem tab && tab.Content is TextBox tb)
                {
                    tb.TextWrapping = TextWrapMenuItem.IsChecked ? TextWrapping.Wrap : TextWrapping.NoWrap;
                }
            }
        }
        #region tab persistence
        
        private string DataFile{
            get {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                string exeFolder = System.IO.Path.GetDirectoryName(exePath);
                return System.IO.Path.Combine(exeFolder, "data", "pages.db");
            }            
        }
        
        private async void UpdateTabsIndexAsync(SQLite.SQLiteAsyncConnection db,
            string? uuid,
            long index) {
            db.ExecuteAsync("UPDATE Page SET `Index` = ? WHERE `Uuid` = ?",index, uuid).Wait();
            long nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            db.ExecuteAsync("UPDATE Page SET `UpdatedAtTimestamp` = ? WHERE `Uuid` = ?", nowTimestamp, uuid).Wait();
        }
        private async void UpdateWorkdSpaceAsync(SQLite.SQLiteAsyncConnection db,
        string? uuid,
        string Workspace)
        {
            db.ExecuteAsync("UPDATE Page SET `Workspace` = ? WHERE `Uuid` = ?", Workspace, uuid).Wait();
            long nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            db.ExecuteAsync("UPDATE Page SET `UpdatedAtTimestamp` = ? WHERE `Uuid` = ?", nowTimestamp, uuid).Wait();
        }
        private async void UpdateSelectedListAsync(SQLite.SQLiteAsyncConnection db,
        string? uuid,
        string selectedList)
        {
            db.ExecuteAsync("UPDATE Page SET `SelectedList` = ? WHERE `Uuid` = ?", selectedList, uuid).Wait();
            long nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            db.ExecuteAsync("UPDATE Page SET `UpdatedAtTimestamp` = ? WHERE `Uuid` = ?", nowTimestamp, uuid).Wait();
        }
        private Model.Page  GetWorkdSpaceAsync(SQLite.SQLiteAsyncConnection db,
        string? uuid)
        {
            var existingPage = db.Table<Model.Page>().FirstOrDefaultAsync(x => x.Uuid == uuid);
            existingPage.Wait();
            return existingPage.Result;
        }

        private Task<int> SaveTabToDatabaseAsync(
            SQLite.SQLiteAsyncConnection db,
            string? uuid,
            string? id,
            string? title,
            string? content)
        {
            int parsedId = 0;
            int.TryParse(id, out parsedId);

            var existingPage = db.Table<Model.Page>().FirstOrDefaultAsync(x => x.Uuid == uuid);
            existingPage.Wait();
            var existingPageResult = existingPage.Result;
            long nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (existingPageResult != null)
            {
                existingPageResult.Title = title ??"";
                existingPageResult.Index = parsedId;
                existingPageResult.Content = content ?? "";
                existingPageResult.UpdatedAtTimestamp = nowTimestamp;
                return db.UpdateAsync(existingPageResult);
                 
            }

            
            Model.Page newPage = new Model.Page
            {
                Uuid = uuid ?? Guid.NewGuid().ToString(),
                Index = parsedId,
                Title = title ?? "",
                Content = content ?? "",
                CreatedAtTimestamp = nowTimestamp
            };
             return db.InsertAsync(newPage);
        }
        private void DeleteTabFileAsync(string? uuid)
        {
            db.Table<Model.Page>().DeleteAsync(x => x.Uuid == uuid);
        }
        private bool isLoadingTabs = false;
        private void RestoreLocation()
        {
            // 选择上次关闭时的标签页
            if (string.IsNullOrEmpty(ctxmgr.Properties.Config.ConfigInstance.LastPage))
            {
                MyTabControl.SelectedIndex = 1;
                return; 
            }
            bool findLast = false;
            foreach (TabItem tab in MyTabControl.Items)
            {
                if (tab.Tag?.ToString() != ctxmgr.Properties.Config.ConfigInstance.LastPage)
                    continue;

                MyTabControl.SelectedItem = tab;
                if (tab.Content is not TextBox tb)
                    return;
                Dispatcher.BeginInvoke(new Action(() => {
                    int caretIndex = ctxmgr.Properties.Config.ConfigInstance.LastCaretIndex;
                    if (caretIndex >= 0 && caretIndex <= tb.Text.Length)
                    {
                        tb.CaretIndex = caretIndex;
                    }
                    else
                    {
                        tb.CaretIndex = tb.Text.Length;
                    }
                    tb.Focus();
                }), System.Windows.Threading.DispatcherPriority.Background);
                findLast = true;
                break;
            }
            if (findLast == false)
            {
                MyTabControl.SelectedIndex = 1;
            }
        }
        private void LoadTabsFromDatabase(SQLite.SQLiteAsyncConnection db)
        {
            isLoadingTabs = true;
            LoadTabsFromDatabaseImpl(db);
            RestoreLocation();
            SyncTabsToMenu();
            isLoadingTabs = false;
           
        }
        private void LoadTabsFromDatabaseImpl(SQLite.SQLiteAsyncConnection db)
        {
            var pagesTask = db.Table<Model.Page>().ToListAsync();
            pagesTask.Wait();
            var pages = pagesTask.Result;
            if (pages.Count == 0)
            {
                var tabItem = CreateNewTabImpl(ctxmgr.Properties.Resources.DefaultPageTitle, "");
                MyTabControl.Items.Add(tabItem);
                tabItem.IsSelected = true;
                return;
            }
            var tabInfos = new List<(long SortKey, TabItem Tab)>();
            foreach (var page in pages)
            {
                /*if (page.Index == 1)
                {
                    DefaultTabItem.Header = page.Title;
                    DefaultTabItem.Tag = page.Uuid;
                    DefaultTextBox.Text = page.Content;
                    TextBoxHelper.SetPlaceholder((TextBox)DefaultTextBox, "");
                    continue;
                }*/
                var tabItem = CreateNewTabImpl(page.Title,page.Content,page.Uuid,page.Workspace);
                tabInfos.Add((page.Index, tabItem));
            }
            foreach (var info in tabInfos.OrderBy(x => x.SortKey))
            {
                MyTabControl.Items.Add(info.Tab);
            }
        }
        #endregion

        private void ChangeTitle_Click(object sender, RoutedEventArgs e)
        {
            if (MyTabControl.SelectedItem == null)
                return;
            var index = MyTabControl.SelectedIndex;
            var tabItem = MyTabControl.SelectedItem as TabItem;
            if (tabItem == null)
                return;
            var result = TabTitleEditorWindow.Show(tabItem?.Header?.ToString(), ctxmgr.Properties.Resources.TabTitleEditor, this);
            if (result == string.Empty)
                return;

            tabItem!.Header = result;

            SyncTabsToMenu();
            SaveTabToDatabaseAsync(db,tabItem?.Tag?.ToString(),
                index.ToString(), tabItem?.Header?.ToString(),
                "");
        }

        private void MainWindowInstance_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePopupPosition(sender,e);
        }

        private void MoveLeftMenuItem_Click(object sender, RoutedEventArgs e)
        {

            if (MyTabControl.Items.Count <= 2 || MyTabControl.SelectedIndex <= 1)
                return;
            var index = MyTabControl.SelectedIndex;
            var previousIndex = index - 1;

            var current = MyTabControl.Items[index] as TabItem;
            if (current == null)
                return;
            var previous = MyTabControl.Items[previousIndex] as TabItem;
            if (previous == null)
                return;
            

            MyTabControl.Items.RemoveAt(index);
            MyTabControl.Items.Insert(previousIndex, current);

            MyTabControl.SelectedItem = current; // 可选：移动后保持选中
            UpdateTabsIndexAsync(db, current.Tag.ToString(), MyTabControl.Items.IndexOf(current));
            UpdateTabsIndexAsync(db, previous.Tag.ToString(), MyTabControl.Items.IndexOf(previous));
        }

        private void MoveRightMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MyTabControl.Items.Count <= 2 || MyTabControl.SelectedIndex >= MyTabControl.Items.Count - 1)
                return;
            var index = MyTabControl.SelectedIndex;
            var nextIndex = index + 1;

            var current = MyTabControl.Items[index] as TabItem;
            if (current == null)
                return;
            var next = MyTabControl.Items[nextIndex] as TabItem;
            if (next == null)
                return;
            UpdateTabsIndexAsync(db, current.Tag.ToString(), nextIndex);
            UpdateTabsIndexAsync(db, next.Tag.ToString(), index);

            MyTabControl.Items.RemoveAt(index);
            MyTabControl.Items.Insert(nextIndex, current);

            MyTabControl.SelectedItem = current; // 可选：移动后保持选中
            UpdateTabsIndexAsync(db, current.Tag.ToString(), MyTabControl.Items.IndexOf(current));
            UpdateTabsIndexAsync(db, next.Tag.ToString(), MyTabControl.Items.IndexOf(next));
        }

        private void MoveLeftMostMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MyTabControl.Items.Count <= 2 || MyTabControl.SelectedIndex <= 1)
                return;
            var index = MyTabControl.SelectedIndex;
            var current = MyTabControl.Items[index] as TabItem;
            
            MyTabControl.Items.RemoveAt(index);
            MyTabControl.Items.Insert(1, current);
            MyTabControl.SelectedItem = current; // 可选：移动后保持选中
            for (int i = 1; i <= index; i++)
            {
                var tab = MyTabControl.Items[i] as TabItem;
                if (tab == null)
                    continue;
                UpdateTabsIndexAsync(db, tab.Tag.ToString(), i);
            }
        }

        private void MoveRightMostMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MyTabControl.Items.Count <= 2 || MyTabControl.SelectedIndex >= MyTabControl.Items.Count - 1)
                return;
            var index = MyTabControl.SelectedIndex;
            var current = MyTabControl.Items[index] as TabItem;


            MyTabControl.Items.RemoveAt(index);
            MyTabControl.Items.Insert(MyTabControl.Items.Count, current);
            MyTabControl.SelectedItem = current; // 可选：移动后保持选中
            for (int i = index; i < MyTabControl.Items.Count; i++)
            {
                var tab = MyTabControl.Items[i] as TabItem;
                if (tab == null)
                    continue;
                UpdateTabsIndexAsync(db, tab.Tag.ToString(), i);
            }
        }
        private TextBox GetCurrentTextBox()
        {
            if (MyTabControl.SelectedItem is TabItem tab && tab.Content is TextBox tb)
                return tb;
            return null;
        }

        private void UndoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTextBox()?.Undo();
        }

        private void CutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTextBox()?.Cut();
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTextBox()?.Copy();
        }

        private void PasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTextBox()?.Paste();
        }
        private string FindingLastKeyword = string.Empty;
        private bool FindingLastMatchCase = false;
        private bool FindingLastWholeWord = false;
        private void FindMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowFindWindowImpl();
        }

        private void ShowFindWindowImpl()
        {
            var tb = GetCurrentTextBox();
            var findWindow = new FindWindow(FindingLastKeyword)
            {
                Owner = this
            };

            // 绑定事件
            findWindow.FindRequested += (keyword, forward, matchCase, wholeWord) =>
            {
                FindingLastKeyword = keyword;
                FindingLastMatchCase = matchCase;
                FindingLastWholeWord = wholeWord;
                return FindTextInTextBox(tb, keyword, forward, matchCase, wholeWord,true);
            };
            //string, bool, bool, bool,string,bool>   // 参数：keyword, forward, matchCase, wholeWord,replaceText
            findWindow.ReplaceRequested += (keyword, forward, matchCase, wholeWord, replaceText) =>
            {
                FindingLastKeyword = keyword;
                FindingLastMatchCase = matchCase;
                FindingLastWholeWord = wholeWord;
                return ReplaceTextInTextBox(keyword, forward, matchCase, wholeWord, replaceText);
            };
            findWindow.ReplacAllRequested += (keyword, replaceText, matchCase, wholeWord) =>
            {
                FindingLastKeyword = keyword;
                FindingLastMatchCase = matchCase;
                FindingLastWholeWord = wholeWord;
                ReplaceAllInTextBox(keyword, replaceText, matchCase, wholeWord);
            };
            findWindow.Show();
        }

        private void FindNextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FindingLastKeyword))
                return;
            var tb = GetCurrentTextBox();
            FindTextInTextBox(tb, FindingLastKeyword, true,
                              FindingLastMatchCase,
                              FindingLastWholeWord);
        }
        private void FindPrevMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FindingLastKeyword))
                return;
            var tb = GetCurrentTextBox();
            
            FindTextInTextBox(tb, FindingLastKeyword, false,
                              FindingLastMatchCase,
                              FindingLastWholeWord);
        }
        private bool FindTextInTextBox(TextBox textBox, string keyword, bool searchForward, bool caseSensitive, bool wholeWord,bool quiet = false)
        {
            if (string.IsNullOrEmpty(keyword)) return true;

            StringComparison comparison = caseSensitive
                ? StringComparison.CurrentCulture
                : StringComparison.CurrentCultureIgnoreCase;

            int? FindNext(int start)
            {
                int idx = textBox.Text.IndexOf(keyword, start, comparison);
                if (idx == -1 && start > 0) 
                    idx = textBox.Text.IndexOf(keyword, 0, comparison);
                return idx == -1 ? (int?)null : idx;
            }

            int? FindPrevious(int start)
            {
                int idx = start >= 0 ? textBox.Text.LastIndexOf(keyword, start, comparison) : -1;
                if (idx == -1 && textBox.Text.Length > 0) 
                    idx = textBox.Text.LastIndexOf(keyword, textBox.Text.Length - 1, comparison);
                return idx == -1 ? (int?)null : idx;
            }

            int? FindValidIndex(int start)
            {
                int textLength = textBox.Text.Length;
                int? current = searchForward ? FindNext(start) : FindPrevious(start);

                while (current != null)
                {
                    if (!wholeWord || IsWholeWord(textBox.Text, current.Value, keyword.Length))
                        return current;

                    int newStart = searchForward ? current.Value + keyword.Length : current.Value - 1;
                    if (newStart < 0 || newStart >= textLength)
                        return null;

                    current = searchForward ? FindNext(newStart) : FindPrevious(newStart);
                }

                return null;
            }
            int startIndex = searchForward
                ? textBox.SelectionStart + textBox.SelectionLength
                : textBox.SelectionStart - 1;

            int? foundIndex = FindValidIndex(startIndex);

            if (foundIndex != null)
            {
                textBox.Select(foundIndex.Value, keyword.Length);
                textBox.ScrollToLine(textBox.GetLineIndexFromCharacterIndex(foundIndex.Value));
                textBox.Focus();
            }
            else
            {   
                if(quiet)
                    return false;
                MessageBoxWindow.Show(string.Format(Properties.Resources.CantFind,FindingLastKeyword),Properties.Resources.FindWindowTitle, MessageBoxButton.OK, this);
            }
            return true;
        }

        // 检查匹配是否是“全词”
        private bool IsWholeWord(string text, int index, int length)
        {
            return IsLeftBoundarySafe(text, start:index) && IsRightBoundarySafe(text, end:index + length);
        }

        private bool IsLeftBoundarySafe(string text, int start) =>
            start == 0 || !IsWordCharacter(text[start - 1]);

        private bool IsRightBoundarySafe(string text, int end) =>
            end == text.Length || !IsWordCharacter(text[end]);

        private bool IsWordCharacter(char c) => char.IsLetterOrDigit(c) || c == '_';
        private bool ReplaceTextInTextBox(string keyword, bool searchForward, bool caseSensitive, bool wholeWord, string replaceText)
        {
            if (string.IsNullOrEmpty(keyword)) return true;
            var textBox = GetCurrentTextBox();
            // 如果当前选中内容就是 keyword → 替换
            if (textBox.SelectedText.Length > 0 &&
                string.Equals(textBox.SelectedText, keyword,
                    caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase) &&
                (!wholeWord || IsWholeWord(textBox.Text, textBox.SelectionStart, keyword.Length)))
            {
                textBox.SelectedText = replaceText;
            }

            // 然后继续查找下一个匹配
            return FindTextInTextBox(textBox, keyword, searchForward, caseSensitive, wholeWord,true);
        }

        private void ReplaceAllInTextBox(string keyword, string replaceText, bool caseSensitive, bool wholeWord)
        {
            if (string.IsNullOrEmpty(keyword)) return;
            var textBox = GetCurrentTextBox();
            // 构造 Regex
            var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            string pattern = Regex.Escape(keyword);

            if (wholeWord)
                pattern = $@"\b{pattern}\b";   // \b 单词边界（英文/数字/下划线 OK）

            string original = textBox.Text;
            string replaced = Regex.Replace(original, pattern, replaceText, options);

            if (!ReferenceEquals(original, replaced))
            {
                int caret = textBox.CaretIndex;
                textBox.Text = replaced;

                // 尝试恢复光标（避免跳到开头）
                textBox.CaretIndex = Math.Min(caret, replaced.Length);
            }
            else
            {
                MessageBoxWindow.Show(
                    string.Format(Properties.Resources.CantFind, keyword),
                    Properties.Resources.FindWindowTitle,
                    MessageBoxButton.OK, this);
            }
        }

        private void ReplaceMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowFindWindowImpl();
        }

        private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTextBox()?.SelectAll();
        }

        private void RedoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentTextBox()?.Redo();
        }

        private void SaveToFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tab = MyTabControl.SelectedItem as TabItem;
            if (tab == null)
                return;
            var tb = GetCurrentTextBox();
            if (tb == null)
                return;
            var uuid = tab.Tag?.ToString();
            var id = MyTabControl.Items.IndexOf(tab).ToString();
            var title = tab.Header?.ToString();
            var content = tb.Text;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = ctxmgr.Properties.Resources.SaveToFile,
                Filter = ctxmgr.Properties.Resources.SaveToFileFilter,
                DefaultExt = ".txt",
                AddExtension = true
            };
            if (saveFileDialog.ShowDialog() != true)
                return;
            string filePath = saveFileDialog.FileName;

            SaveTabToFileAsync(uuid, id, title, content, filePath);
        }
        private TabItem CreateNewTabImpl(string header,string content,string uuid = null,string tooltip = null)
        {
            var newTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = ctxmgr.Properties.Config.ConfigInstance.TextWrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
                Text = content,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            TextBoxHelper.SetPlaceholder((TextBox)newTextBox, "");
            newTextBox.TextChanged += TextBox_TextChanged;
            newTextBox.LostFocus += DefaultTextBox_LostFocus;
            newTextBox.PreviewKeyDown += DefaultTextBox_PreviewKeyDown;
            
            if (uuid == null)
                uuid = Guid.NewGuid().ToString();
            var tabItem = new TabItem
            {
                Header = header,
                Tag = uuid,
                Content = newTextBox
            };
            //if (!string.IsNullOrEmpty(tooltip))
            //    tabItem.ToolTip = tooltip;
            tabItem.HeaderTemplate = this.FindResource("DoubleClickableHeader") as DataTemplate;
            return tabItem;
        }
        private  async void LoadFromFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = ctxmgr.Properties.Resources.OpenFile,
                Filter = ctxmgr.Properties.Resources.OpenFileFilter,
                Multiselect = false 
            };
            if (openFileDialog.ShowDialog() != true)
                return;
            
            string filePath = openFileDialog.FileName;
            var readTask = System.IO.File.ReadAllLinesAsync(filePath);
            readTask.Wait();
            var lines = readTask.Result;
            if (lines.Length < 2)
                return;

            string id = lines[0];
            string title = lines[1];
            string content = string.Join("\n", lines.Skip(2));
            isLoadingTabs = true;
            var tabItem = CreateNewTabImpl(title,content);
            SaveTabToDatabase(MyTabControl.SelectedItem, false);
            isLoadingTabs = false;
            MyTabControl.Items.Add(tabItem);
        }
        private async Task SaveTabToFileAsync(
            string? uuid,
            string? id,
            string? title,
            string? content,
            string filePath)
        {
            string text = $"{id}\n{title}\n{content}";
            await System.IO.File.WriteAllTextAsync(filePath, text);
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ctxmgr.Page.Settings.SettingsWindow.Show(this,false, _hotkeyManager.UpdateHotkey);
        }


        private void TabItemHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Properties.Config.ConfigInstance.DoubleClickTitleAction == Properties.DoubleClickTitleActionEnum.None)
                return;
            if (Properties.Config.ConfigInstance.DoubleClickTitleAction == Properties.DoubleClickTitleActionEnum.EditTitle)
            {
                ChangeTitle_Click(sender, e);
            }
            else if (Properties.Config.ConfigInstance.DoubleClickTitleAction == Properties.DoubleClickTitleActionEnum.DeletePage)
            {
                DelTab_Click(sender, e);
            }
        }

        private void InsertTextSettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ctxmgr.Page.Settings.SettingsWindow.Show(this,true, _hotkeyManager.UpdateHotkey);
        }
        private void InsertCustomerTextSnippetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            var menuItem = sender as MenuItem;
            if(menuItem == null)
                return;
            var data = menuItem.DataContext as CustomerTextSnippet;
            if(data == null)
                return;

            InsertTextAtCursor(data.Text);
        }

        private void FontSettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FontSettingsWindow.Show(this);
        }

        private void BgColorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            BackgroundColorWindow.Show(this);
        }


        private void DefaultTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
            if (!(Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter))
                return;
            if (sender is not TextBox tb)
                return;
            e.Handled = true;//阻止默认换行
            int lineIndex = tb.GetLineIndexFromCharacterIndex(tb.CaretIndex);
            if (lineIndex < 0) return;
            try
            {
                /*
                string lineText = tb.GetLineText(lineIndex);

                var expr = new NCalc.Expression(lineText.Trim());
                var result = expr.Evaluate();

                int lineStart = tb.GetCharacterIndexFromLineIndex(lineIndex);
                int lineEnd = lineStart + lineText.Length;

                tb.Select(lineEnd, 0);
                tb.SelectedText = " = " + result?.ToString();*/
            //    int lineIndex = tb.GetLineIndexFromCharacterIndex(tb.CaretIndex);
                int lineStart = tb.GetCharacterIndexFromLineIndex(lineIndex);
                int lineLength = tb.GetLineLength(lineIndex);

                // 获取当前行文本
                string currentLine = tb.Text.Substring(lineStart, lineLength);
                string lineContent = currentLine.TrimEnd('\r', '\n');
                string lineEnding = currentLine.Substring(lineContent.Length);
                var expr = new NCalc.Expression(currentLine.Trim());
                var result =  expr.Evaluate(); // result 在这里定义
                
                // 构造新行
                int equalIndex = currentLine.IndexOf('=');
                string newLine;
                if (equalIndex >= 0)
                {
                    newLine = lineContent + " = " + result?.ToString();
                }
                else
                {
                    newLine = lineContent + " = " + result?.ToString();
                }

                // 替换整行文本
                bool isLastLine = (lineIndex == tb.LineCount - 1);
                
                tb.Select(lineStart, isLastLine? lineLength:lineLength - 2);
                tb.SelectedText = newLine;

                // 光标移动到行末，不选中
                //tb.SelectionLength = 0;
                tb.CaretIndex = lineStart + newLine.Length;

            }
            catch (Exception ex) { 

            }
        }
        private void SaveTabToDatabase(object sender, bool waitTabItem = true)
        {
            var tabItem = sender as TabItem;
            if (tabItem == null)
                return;

            SaveTabToDatabaseImpl(tabItem,waitTabItem);
        }
        private void SaveTbsTabToDatabase(object sender, bool waitTabItem)
        {
            var tb = sender as TextBox;
            if (tb == null)
                return;
            var tabItem = ItemsControl.ContainerFromElement(MyTabControl, tb) as TabItem;
            if (tabItem == null)
                return;
            SaveTabToDatabaseImpl(tabItem, waitTabItem);
        }
        private void SaveTabToDatabaseImpl(TabItem tabItem, bool waitTabItem)
        {
            if (tabItem == menuTabItem)
                return;
            if (!TextChangedTabItems.ContainsKey(tabItem))
                return;
            TextChangedTabItems.Remove(tabItem);
            if (tabItem == null)
                return;
            var tb = tabItem.Content as TextBox;
            if (tb == null)
                return;
            string? uuid = tabItem.Tag.ToString();
            if (string.IsNullOrEmpty(uuid))
                return;

            var data = new TabData(
                    uuid,
                    MyTabControl.Items.IndexOf(tabItem).ToString(),
                    tabItem.Header.ToString()!,
                    tb.Text
            );
            if (tabItem != null && waitTabItem)
            {
                SaveTabToDatabaseAsync(db, data.Uuid, data.Id,
                    data.Title, data.Content).Wait();
            }
            else
            {
                _ = SaveTabToDatabaseAsync(db, data.Uuid, data.Id,
                    data.Title, data.Content);
            }
        }
        private  void DefaultTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if(isLoadingTabs)
                return;

            SaveTbsTabToDatabase(sender,false);
        }

        private void ShortcutListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var shortCutWindow = new ctxmgr.Page.Shortcut.ShortcutWindow
            {
                Owner = this
            };
            shortCutWindow.Show();
        }

        private void GenerateCtxBtn_Click(object sender, RoutedEventArgs e)
        {
            var curTab = MyTabControl.SelectedItem as TabItem;
            if (curTab == null)
                return;

            var uuid = curTab.Tag.ToString();
            if (string.IsNullOrEmpty(uuid))
                return;
            var page = GetWorkdSpaceAsync(db, uuid);

            if (page == null) return;

            var tb = GetCurrentTextBox();
            var prompt = new StringBuilder();
            if (ctxmgr.Properties.Config.ConfigInstance.PromptAtHeader)
                prompt.Append( tb.Text+"\n\n");
            
            prompt.Append(JointSelectFolderFiles(page));
            
            if(ctxmgr.Properties.Config.ConfigInstance.PromptAtFooter)
                prompt.Append(tb.Text);
            //  ***“任务描述靠前，参考资料靠后”** → 常用于问答、代码生成。
            //  ***“参考资料靠前，任务描述靠后”** → 常用于总结、提炼。
            //  可以前后都加
            //Dispatcher.BeginInvoke(() =>
            //{

            ctxmgr.Utilities.NativeClipboard.SetText(prompt.ToString());
                new ToastWindow(ctxmgr.Properties.Resources.ContextCopiedSuccessfully).Show();
            //});
        }
        static StringBuilder JointSelectFolderFiles(Model.Page page)
        {
            if (page.SelectedListItems == null || page.SelectedListItems.Count == 0)
            {
                return new StringBuilder();
            }
            var paths = page.SelectedListItems?.Select(x => Path.Combine(page.Workspace, x.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))).ToList();
            var fileDict = paths
                .SelectMany(GetAllFiles) // 展开所有文件
                .Where(System.IO.File.Exists)
                .ToDictionary(
                    path => path,
                    path => new StringBuilder(SafeReadAllText(path))
                );

            // 拼接大字符串
            var result = new StringBuilder(
                string.Join(
                    Environment.NewLine,
                    fileDict.Select(kv => $"{kv.Key}:\n<code>\n{kv.Value}\n</code>\n")
                )
            );
            return result;
        }
        static IEnumerable<string> GetAllFiles(string path)
        {
            if (System.IO.File.Exists(path))
            {
                yield return path;
            }
            else if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    yield return file;
                }
            }
        }
        static string SafeReadAllText(string path)
        {
            try
            {
                return System.IO.File.ReadAllText(path);
            }
            catch
            {
                return "";
            }
        }

    }

}