using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ctxmgr.Page.FileFolderSelector
{
    /// <summary>
    /// Interaction logic for FileFolderSelector.xaml
    /// </summary>
    public partial class FileFolderSelector : Window
    {
        private FileFolderSelector()
        {
            InitializeComponent();
        }
        
        public FileFolderSelector(Action<String> setWorkSpaceAction,string workSpace, string uuid,Action<string> writeSelectedListAction)
        {
            InitializeComponent();
            var vm = new MainViewModel();
            
            vm.SetWorkSpace += setWorkSpaceAction;
            vm.WriteSelectedList += writeSelectedListAction;
            vm.TargetFolder = workSpace;
            vm.LoadCommand.Execute(null);
            this.DataContext = vm;
            this.FolderTextBox.Text = workSpace;
            FolderTextBox.Focus();
            FolderTextBox.SelectAll();
            
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) { e.Handled = true; Close(); }
            };

        }


        private void TreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TreeView treeView)
            {
                // 获取点击的TreeViewItem
                //e.OriginalSource：获取鼠标点击的原始UI元素
                //可能是TextBlock、CheckBox、Border等任意子元素
                //需要找到包含这些元素的TreeViewItem
                var clickedItem = GetClickedTreeViewItem(e.OriginalSource as DependencyObject);
                /*
                DependencyObject 是WPF中所有UI元素的基类
                System.Object
                └── System.Windows.DependencyObject
                    └── System.Windows.UIElement
                        └── System.Windows.FrameworkElement
                            └── System.Windows.Controls.Control
                                └── System.Windows.Controls.ContentControl
                                    └── System.Windows.Controls.Button

                */


                if (clickedItem != null && clickedItem.DataContext is FileSystemItemViewModel item)
                {
                    // 只对文件启用双击功能，不对文件夹
                    if (!item.IsDirectory)
                    {
                        // 切换复选框状态
                        item.IsChecked = !item.IsChecked;
                        e.Handled = true;
                    }
                }
            }
        }

        private TreeViewItem GetClickedTreeViewItem(DependencyObject source)
        {
            while (source != null && source is not TreeViewItem)
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source as TreeViewItem;
        }

        private void FolderTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            // 可选：阻止事件继续传递（如避免换行）
            e.Handled = true;
            if (this.DataContext is MainViewModel vm)
            {
                vm.LoadCommand.Execute(null);
            }
        }
    }
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string targetFolder;

        public ObservableCollection<FileSystemItemViewModel> RootItems { get; } = new ObservableCollection<FileSystemItemViewModel>();
        
        [RelayCommand]
        private void Browse()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    TargetFolder = dialog.SelectedPath;
                    LoadImpl();
                }
            }
        }
        public event Action<string> SetWorkSpace;
        public event Action<string> WriteSelectedList;
        private void LoadImpl()
        {
            if (!Directory.Exists(TargetFolder))
            {
                System.Windows.MessageBox.Show("目标文件夹不存在！");
                return;
            }
            SetWorkSpace(TargetFolder);
            RootItems.Clear();
            RootItems.Add(new FileSystemItemViewModel
            {
                Name = System.IO.Path.GetFileName(TargetFolder.TrimEnd('\\')),
                FullPath = TargetFolder,
                IsDirectory = true
            });
            FileSystemItemViewModel.WorkSpace += TargetFolder;
            FileSystemItemViewModel.GetAllChecked += GetAllCheckedNode;
            FileSystemItemViewModel.WriteSelectedList += WriteSelectedList;
        }
        private List<FileSystemItemViewModel> GetAllCheckedNode()
        {
            var list = new List<FileSystemItemViewModel>();
            void Traverse(FileSystemItemViewModel node)
            {
                if (node.IsChecked == false)
                    return;
                if (node.IsChecked == true)
                {
                    if (node.IsDirectory)
                    {
                        list.Add(node);
                    }
                    else
                        list.Add(node);
                }
                else if (node.IsChecked == null)
                {
                    if (!node.IsDirectory)
                        return;
                    if (node.Children != null)
                    {
                        foreach (var child in node.Children)
                        {
                            if (child == null)
                                continue;
                            Traverse(child);
                        }
                    }
                }
            }
            foreach (var root in RootItems)
                Traverse(root);
            return list;
        }
        [RelayCommand]
        private void Load()
        {
            LoadImpl();
        }
        [RelayCommand]
        private void Reset() {
            void Traverse(FileSystemItemViewModel node)
            {
                if (node == null) return;
                if (node.IsDirectory)
                {
                    foreach (var child in node.Children)
                    {
                        Traverse(child);
                    }
                    node.IsChecked = false;
                }
                else
                {
                    node.IsChecked = false;
                }
            }
            foreach (var root in RootItems)
                Traverse(root);
        }
    }
}
