using ctxmgr.Model;
using Microsoft.VisualBasic;
using SQLite;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace ctxmgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DatabaseService service = new ctxmgr.Model.DatabaseService();
        SQLite.SQLiteAsyncConnection db = null;
        private GlobalHotkeyManager _hotkeyManager;
        private Properties.Config ConfigInstance = null;
        public MainWindow()
        {
            InitializeComponent();
            this.ToggleTopmost.IsChecked = this.Topmost;
            // 恢复窗口位置 
            ConfigInstance = Properties.Config.Load();
            this.Topmost = ConfigInstance.StayOnTop;
            this.ToggleTopmost.IsChecked = ConfigInstance.StayOnTop;

            {
                bool isAutoStartEnabled = AutoStartHelper.IsAutoStartEnabled();
                this.ToggleRunOnStartUp.IsChecked = isAutoStartEnabled;
            }
            #region textwrap
            TextWrapMenuItem.IsChecked = ConfigInstance.TextWrap;
            DefaultTextBox.TextWrapping = ConfigInstance.TextWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
            foreach (var item in MyTabControl.Items)
            {
                if (item is TabItem tab && tab.Content is TextBox tb)
                {
                    tb.TextWrapping = TextWrapMenuItem.IsChecked ? TextWrapping.Wrap : TextWrapping.NoWrap;
                }
            }
            #endregion
            if (ConfigInstance.WindowLeft >= 0 && ConfigInstance.WindowTop >= 0)
            {
                // 虚拟桌面范围（所有显示器合并后的区域）
                double left = ConfigInstance.WindowLeft;
                double top = ConfigInstance.WindowTop;
                double right = left + this.Width;
                double bottom = top + this.Height;
                double width = ConfigInstance.WindowWidth > 0 ? ConfigInstance.WindowWidth : this.Width;
                double height = ConfigInstance.WindowHeight > 0 ? ConfigInstance.WindowHeight : this.Height;


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
            DataObject.AddPastingHandler(DefaultTextBox, OnPasting);

            _hotkeyManager = new GlobalHotkeyManager();
            _hotkeyManager.HotkeyPressed += OnHotkeyPressed;
            _hotkeyManager.HotkeyAppendPressed += _hotkeyManager_HotkeyAppendPressed; ;
            Loaded += (s, e) =>
            {
                _hotkeyManager.Register(this);
                #region 自动聚焦到当前激活Tab的TextBox
                if (MyTabControl.SelectedItem is TabItem selectedTab)
                {
                    if (selectedTab.Content is TextBox textBox)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                        {
                            textBox.Focus();
                            textBox.CaretIndex = textBox.Text.Length;
                        }));
                    }
                }
                #endregion
            };
            System.Windows.Application.Current.Exit += (s, e) => _hotkeyManager.Dispose();
            var dbInitTask = service.OpenOrCreateDatabase(DataFile);
            dbInitTask.Wait();
            db = dbInitTask.Result;
            service.CreateTables(db).Wait();
            LoadTabsFromDatabase(db);
        }

        private void _hotkeyManager_HotkeyAppendPressed(object? sender, ClipEventArgs e)
        {
            string curText = DefaultTextBox.Text;
            if (curText.Trim('\r').EndsWith(e.Message))
                return;
            DefaultTextBox.AppendText(e.Message);
            DefaultTextBox.ScrollToEnd();
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            if (this.Visibility != Visibility.Hidden)
            {
                this.Hide();
                return;
            }

            this.Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
        }

        #region Esc hide window
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;
            this.Hide();
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
            else if (File.Exists(trimmedPath))
            {
                return $"[x]@File {trimmedPath}\n";
            }

            if (Directory.Exists(path))
            {
                return $"[x]@Folder {path}\n";
            }
            else if (File.Exists(path))
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
            if (!e.DataObject.GetDataPresent(DataFormats.Text)) return;

            string raw = (string)e.DataObject.GetData(DataFormats.Text);
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
        private DispatcherTimer _saveTimer; // 用于实现防抖的定时器
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
        private Dictionary<string, TabData> ChangedTabItems = new Dictionary<string, TabData>();
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isLoadingTabs)
                return;
            if (sender is not TextBox tb)
                return;
            if (MyTabControl.SelectedItem is not TabItem selectedTab)
                return;
            string? uuid = selectedTab.Tag.ToString();
            if (string.IsNullOrEmpty(uuid))
                return;
            
            ChangedTabItems[uuid]= new TabData (
                    uuid,
                    MyTabControl.Items.IndexOf(selectedTab).ToString(),
                    selectedTab.Header.ToString()!,
                    tb.Text
            );

            async void SaveTimerHandler(object? s, EventArgs e)
            {
                _saveTimer.Stop();
                // 复制并清空字典，避免并发修改
                var itemsToSave = new Dictionary<string, TabData>(ChangedTabItems);
                ChangedTabItems.Clear();
                foreach (var kv in itemsToSave)
                {
                    var savedTab = kv.Value;

                    SaveTabToFileAsync(db, savedTab.Uuid, savedTab.Id,
                        savedTab.Title, savedTab.Content);
                }
            }
            if (_saveTimer == null)
            {
                _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2500) };
                _saveTimer.Tick += SaveTimerHandler;
            }
            _saveTimer!.Stop();
            _saveTimer!.Start();
            UpdatePopupPosition(sender, e);
            //SuggestionsPopup.IsOpen = !SuggestionsPopup.IsOpen;
            
        }



        #region topmost
        private void ToggleTopmost_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
            this.ToggleTopmost.IsChecked = this.Topmost;
            ConfigInstance.StayOnTop = this.Topmost;
            ConfigInstance.Save();
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
        private static void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
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

        private About aboutDialog = new About();
        private void About_Click(object sender, RoutedEventArgs e)
        {
            aboutDialog.Owner = this;
            aboutDialog.Show();
        }

        private void HideWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            ConfigInstance.WindowLeft = this.Left;
            ConfigInstance.WindowTop = this.Top;
            ConfigInstance.WindowWidth = this.Width;
            ConfigInstance.WindowHeight = this.Height;
            ConfigInstance.Save();
            _saveTimer?.Stop();
            _saveTimer = null;
            System.Windows.Application.Current.Shutdown();
        }

        private void LightMode_Click(object sender, RoutedEventArgs e)
        {
            SwitchTheme(false);
        }

        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            SwitchTheme(true);
        }
        public void SwitchTheme(bool isDarkMode)
        {
            var uri = isDarkMode
                ? new Uri("DarkTheme.xaml", UriKind.Relative)
                : new Uri("LightTheme.xaml", UriKind.Relative);

            var newTheme = new ResourceDictionary { Source = uri };
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(newTheme);
        }
        //loading path AppData\Local\ctxmgr\ctxmgr_Url_qkyp43qq1dqf12ub3mpsvwt1inkryzoa\1.0.0.0
        //%APPDATA%\[公司名]\[程序名]_Url_[随机字符串]\[版本号]\user.config

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ConfigInstance.WindowLeft = this.Left;
            ConfigInstance.WindowTop = this.Top;
            ConfigInstance.WindowWidth = this.Width;
            ConfigInstance.WindowHeight = this.Height;
            ConfigInstance.Save();

            e.Cancel = true;
            this.Hide();
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
            string uuid = Guid.NewGuid().ToString();
            var newTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = ConfigInstance.TextWrap ? TextWrapping.Wrap : TextWrapping.NoWrap
            };
            newTextBox.TextChanged += TextBox_TextChanged;
            var newItem = new TabItem
            {
                Header = $"Tab {MyTabControl.Items.Count}",
                Tag = uuid,
                Content = newTextBox

            };
            SaveTabToFileAsync(db,newItem?.Tag?.ToString(),
                MyTabControl.Items?.Count.ToString(), newItem?.Header?.ToString(),
                "");
            TextBoxHelper.SetPlaceholder((TextBox)newItem!.Content, ctxmgr.Properties.Resources.TextBoxHint);
            MyTabControl!.Items!.Add(newItem);
            MyTabControl.SelectedItem = newItem;
            SyncTabsToMenu();
        }

        private void DelTab_Click(object sender, RoutedEventArgs e)
        {
            if (MyTabControl.Items.Count <= 2)
                return;
            if (MessageBox.Show("你确定要继续吗？", "确认", MessageBoxButton.YesNo, this) == MessageBoxResult.Yes)
            {
                var oldSelectedIndex = MyTabControl.SelectedIndex;
                if (MyTabControl.SelectedItem != null)
                {
                    var tabItem = MyTabControl.SelectedItem as TabItem;
                    var textBox = tabItem.Content as TextBox;
                    if (textBox != null)
                        textBox.TextChanged -= TextBox_TextChanged;
                    var uuid  = tabItem?.Tag?.ToString();
                    if (uuid != null)
                    {
                        ChangedTabItems.Remove(uuid);
                    }
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
            if (MyTabControl.Items.Count <= 2)
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
                        // Optionally move caret to end of text
                        textBox.CaretIndex = textBox.Text.Length;
                    }));
                }
            }
            #endregion
        }


        private void MainWindowInstance_LocationChanged(object sender, EventArgs e)
        {
            if (SuggestionsPopup.IsOpen)
            {
                UpdatePopupPosition(sender,e);
            }
        }
        private void UpdatePopupPosition(object sender, EventArgs e)
        {
            if (SuggestionsPopup.IsOpen)
            {
                SuggestionsPopup.HorizontalOffset = MainWindowInstance.Left;
                SuggestionsPopup.VerticalOffset = MainWindowInstance.Top + MainWindowInstance.ActualHeight;
            }
        }
        private void InsertSeparatorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Get the currently active TextBox
            TabItem selectedTab = MyTabControl.SelectedItem as TabItem;
            if (selectedTab == null) return;

            TextBox activeTextBox = selectedTab.Content as TextBox;
            if (activeTextBox == null) return;

            // Create a separator line (80 dashes)
            string separator = new string('-', 80);

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
            var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            InsertTextAtCursor(currentDateTime);
        }

        private void InsertDateTimeSeparator_Click(object sender, RoutedEventArgs e)
        {
            // Get the currently active TextBox
            TabItem selectedTab = MyTabControl.SelectedItem as TabItem;
            if (selectedTab == null) return;

            TextBox activeTextBox = selectedTab.Content as TextBox;
            if (activeTextBox == null) return;
            var separator = new string('-', 20);
            var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var formattedText = $"{separator} {currentDateTime} {separator}";
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

        private void ToggleRunOnStartUp_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleRunOnStartUp.IsChecked = !this.ToggleRunOnStartUp.IsChecked;

            AutoStartHelper.SetAutoStart(this.ToggleRunOnStartUp.IsChecked == true);
            ConfigInstance.RunOnStartUp = this.ToggleRunOnStartUp.IsChecked == true;
            ConfigInstance.Save();
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
            ConfigInstance.TextWrap = TextWrapMenuItem.IsChecked;
            ConfigInstance.Save();

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
        private string DataFolder => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        private string DataFile => System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.db");
        private string GetTabFilePath(string uuid) => System.IO.Path.Combine(DataFolder, $"{uuid}.txt");
        private async void UpdateTabsIndexAsync(SQLite.SQLiteAsyncConnection db,
            string? uuid,
            long index) {
            db.ExecuteAsync("UPDATE Page SET `Index` = ? WHERE `Uuid` = ?",index, uuid).Wait();
        }
        private async void SaveTabToFileAsync(
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
            if (existingPageResult != null)
            {
                existingPageResult.Title = title ??"";
                existingPageResult.Index = parsedId;
                existingPageResult.Content = content ?? "";
                db.UpdateAsync(existingPageResult);
                return;
            }

            Model.Page newPage = new Model.Page
            {
                Uuid = uuid ?? Guid.NewGuid().ToString(),
                Index = parsedId,
                Title = title ?? "",
                Content = content ?? ""
            };
            db.InsertAsync(newPage);
        }
        private void DeleteTabFileAsync(string? uuid)
        {
            db.Table<Model.Page>().DeleteAsync(x => x.Uuid == uuid);
        }
        private bool isLoadingTabs = false;
        private void LoadTabsFromDatabase(SQLite.SQLiteAsyncConnection db)
        {
            isLoadingTabs = true;
            LoadTabsFromDatabaseImpl(db);
            isLoadingTabs = false;
        }
        private void LoadTabsFromDatabaseImpl(SQLite.SQLiteAsyncConnection db)
        {
            var pagesTask = db.Table<Model.Page>().ToListAsync();
            pagesTask.Wait();
            var pages = pagesTask.Result;
            if (pages.Count == 0)
            {
                string uuid = Guid.NewGuid().ToString();
                DefaultTabItem.Tag = uuid;
                return;
            }
            var tabInfos = new List<(long SortKey, TabItem Tab)>();
            foreach (var page in pages)
            {
                if (page.Index == 1)
                {
                    DefaultTabItem.Header = page.Title;
                    DefaultTabItem.Tag = page.Uuid;
                    DefaultTextBox.Text = page.Content;
                    TextBoxHelper.SetPlaceholder((TextBox)DefaultTextBox, "");
                    continue;
                }
                var newTextBox = new TextBox
                {
                    AcceptsReturn = true,
                    TextWrapping = ConfigInstance.TextWrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
                    Text = page.Content
                };
                TextBoxHelper.SetPlaceholder((TextBox)newTextBox, "");
                newTextBox.TextChanged += TextBox_TextChanged;

                var tabItem = new TabItem
                {
                    Header = page.Title,
                    Tag = page.Uuid,
                    Content = newTextBox
                };
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
            var result = TabTitleEditor.Show(tabItem?.Header?.ToString(), ctxmgr.Properties.Resources.TabTitleEditor, this);
            if (result == string.Empty)
                return;

            tabItem!.Header = result;

            SyncTabsToMenu();
            SaveTabToFileAsync(db,tabItem?.Tag?.ToString(),
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
    }
}