using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace ctxmgr.Page.FileFolderSelector
{
    public partial class FileSystemItemViewModel: ObservableObject
    {
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

        partial void OnIsExpandedChanged(bool value)
        {
            OnPropertyChanged(nameof(Icon));
            if (value && IsDirectory && Children.Count == 1 && Children[0] == null)
                LoadChildren();
        }

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
                // 用户点击文件夹时，全选或全不选
                foreach (var child in Children)
                    child.IsChecked = value;
            }

            // 更新父节点状态（父节点显示三态仅由子节点决定）
            Parent?.UpdateCheckState();
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
    }
}
