using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Media;

namespace ctxmgr.Page.FileFolderSelector
{
    public partial class FileSystemItemViewModel: ObservableObject
    {
        public static string WorkSpace;
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool? _isChecked = false; // 三态

        private ObservableCollection<FileSystemItemViewModel> _children;
        public ObservableCollection<FileSystemItemViewModel> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<FileSystemItemViewModel>();
                    if (IsDirectory)
                        _children.Add(null); // 占位，懒加载
                }
                return _children;
            }
            set => SetProperty(ref _children, value);
        }

        public FileSystemItemViewModel Parent { get; set; }

        public string Icon => IsDirectory ? (IsExpanded ? "📂" : "📁") : "📄";
        public Brush IconColor => IsDirectory ? Brushes.Orange : Brushes.Gray;

        partial void OnIsExpandedChanged(bool value)
        {
            OnPropertyChanged(nameof(Icon));
            //Line:32
            if (value && IsDirectory && NotLoading)
                LoadChildren();
        }
        public static event Func<List<FileSystemItemViewModel>> GetAllChecked;
        public static event Action<string> WriteSelectedList;
        public static bool IsLoading = true;
        partial void OnIsCheckedChanged(bool? value)
        {
            
            // 防止用户手动点击时出现 null
            if (!value.HasValue)
            {
                // 强制把 null 转为 false
                value = false;
                _isChecked = false;
                OnPropertyChanged(nameof(IsChecked));
            }

            if (IsDirectory && Children != null)
            {
                if (NotLoading)
                    LoadChildren();
                // 用户点击文件夹时，全选或全不选
                foreach (var child in Children)
                    child.IsChecked = value;
            }

            // 更新父节点状态（父节点显示三态仅由子节点决定）
            Parent?.UpdateCheckState();
            if (IsLoading) return;
            //Gen list
            var items = GetAllChecked();
            
            List<string> pathList = new List<string>();
            foreach (var item in items)
            {
                pathList.Add(item.FullPath.Substring(WorkSpace.Length));
            }
            WriteSelectedList(JsonSerializer.Serialize(pathList));
        }

        private void LoadChildren()
        {
            Children.Clear();
            try
            {
                foreach (var dir in Directory.GetDirectories(FullPath))
                {
                    Children.Add(new FileSystemItemViewModel
                    {
                        Name = Path.GetFileName(dir),
                        FullPath = dir,
                        IsDirectory = true,
                        Parent = this
                    });
                }

                foreach (var file in Directory.GetFiles(FullPath))
                {
                    Children.Add(new FileSystemItemViewModel
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file,
                        IsDirectory = false,
                        Parent = this
                    });
                }
            }
            catch { /* 忽略权限异常 */ }
        }

        public void UpdateCheckState()
        {
            if (!IsDirectory || Children == null || Children.Count == 0) return;

            int checkedCount = Children.Count(c => c.IsChecked == true);
            int uncheckedCount = Children.Count(c => c.IsChecked == false);

            if (checkedCount == Children.Count)
                _isChecked = true;
            else if (uncheckedCount == Children.Count)
                _isChecked = false;
            else
                _isChecked = null;

            OnPropertyChanged(nameof(IsChecked));
            Parent?.UpdateCheckState();
        }
        #region 唯一入口
        /// <summary>
        /// relativePaths：相对于 WorkSpace 的路径集合，可文件可目录。
        /// </summary>
        public void EnsureChecked(List<string> paths)
        {
            if (paths == null || paths.Count == 0)
            {
                FileSystemItemViewModel.IsLoading = false; 
                return;
            }
                
            FileSystemItemViewModel.IsLoading = true;
            EnsureCheckedImpl(paths);
            FileSystemItemViewModel.IsLoading = false;
        }
        public void EnsureCheckedImpl(List<string> paths)
        {
            // 1+2）直接展开——现场判断前缀
            ExpandIfPrefixOfAny(paths);

            // 3）打勾 + 修正祖先三态
            CheckLeaves(paths);
        }
        #endregion

        #region 展开：只要当前路径是任一预选路径的前缀就继续
        private void ExpandIfPrefixOfAny(List<string> paths)
        {
            if (!IsDirectory) return;

            // 现场前缀匹配
            bool needLoad = paths.Any(path => path.StartsWith(this.FullPath));//, StringComparison.Ordinal
            if (!needLoad) return;

            // 展开
            if (NotLoading)
                LoadChildren();

            // 继续深入
            foreach (var child in Children)
                child.ExpandIfPrefixOfAny(paths);
        }
        private bool NotLoading => Children.Count == 1 && Children[0] == null;
        #endregion

        #region 打勾叶子 + 自底向上修正
        private void CheckLeaves(List<string> paths)
        {
            //;
            if (paths.Contains(this.FullPath))
            {
                this.IsChecked = true; 
                paths.Remove(this.FullPath);
            }
            foreach (var child in this.Children)
            {
                if(child ==  null) continue;
                child.CheckLeaves(paths);
            }
        }
        #endregion

    }
}
