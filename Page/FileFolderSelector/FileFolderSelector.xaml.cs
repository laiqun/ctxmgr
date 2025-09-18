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
        public FileFolderSelector()
        {
            InitializeComponent();
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) { e.Handled = true; Close(); }
            };
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
                    TargetFolder = dialog.SelectedPath;
            }
        }

        [RelayCommand]
        private void Load()
        {
            if (!Directory.Exists(TargetFolder))
            {
                System.Windows.MessageBox.Show("目标文件夹不存在！");
                return;
            }

            RootItems.Clear();
            RootItems.Add(new FileSystemItemViewModel
            {
                Name = System.IO.Path.GetFileName(TargetFolder),
                FullPath = TargetFolder,
                IsDirectory = true
            });
        }
    }
}
