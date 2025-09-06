using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ctxmgr.Properties;
using ctxmgr.Utilities;

namespace ctxmgr.Page.Settings
{
    public partial class SettingsViewModel:ObservableObject
    {
        [ObservableProperty]
        private bool isAutoStart;
        partial void OnIsAutoStartChanged(bool value)
        {
            AutoStartHelper.SetAutoStart(isAutoStart);
            Properties.Config.ConfigInstance.RunOnStartUp = isAutoStart;
            Properties.Config.ConfigInstance.Save();
        }
        [ObservableProperty]
        private Properties.DoubleClickTitleActionEnum doubleClickTitleAction;

        partial void OnDoubleClickTitleActionChanged(DoubleClickTitleActionEnum value)
        {
            Properties.Config.ConfigInstance.DoubleClickTitleAction = value;
        }
        public SettingsViewModel()
        {
            isAutoStart = Properties.Config.ConfigInstance.RunOnStartUp;
            doubleClickTitleAction = Properties.Config.ConfigInstance.DoubleClickTitleAction;
        }
    }
}
