using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ctxmgr.Utilities;

namespace ctxmgr.Page.Settings
{
    public class SettingsViewModel:ObservableObject
    {
        private bool _isAutoStart;
        public bool IsAutoStart
        {
            get => _isAutoStart;
            set
            {
                if (SetProperty(ref _isAutoStart, value))
                {
                    // 当属性值改变时，执行相应的逻辑
                    bool result = AutoStartHelper.SetAutoStart(value);
                    if (!result)
                    {
                        // 如果设置失败，可以选择回滚属性值
                        _isAutoStart = !value;
                        OnPropertyChanged(nameof(IsAutoStart));
                    }
                }
            }
        }
        public SettingsViewModel()
        {
            // 初始化时检查当前的自启动状态
            _isAutoStart = CheckAutoStartStatus();
        }
        private bool CheckAutoStartStatus()
        {
            // 这里可以实现检查当前是否设置了开机自启动的逻辑
            // 例如检查启动文件夹中是否存在对应的快捷方式
            string shortcutFullPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Startup), "FlashPad.lnk");
            return System.IO.File.Exists(shortcutFullPath);
        }
    }
}
