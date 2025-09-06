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
        [ObservableProperty]
        private string insertLineText;
        partial void OnInsertLineTextChanged(string value)
        {
            Properties.Config.ConfigInstance.InsertLineText = value;
        }

        [ObservableProperty]
        private string insertDateText;
        partial void OnInsertDateTextChanged(string value)
        {
            Properties.Config.ConfigInstance.InsertDateText = value;
        }


        [ObservableProperty]
        private string insertLineDateText;
        partial void OnInsertLineDateTextChanged(string value)
        {
            Properties.Config.ConfigInstance.InsertLineDateText = value;
        }

        public void ResetToDefault()
        {
            var insertLineText = "crlf() + chars('-', 80) + crlf()";
            var insertDateText = "yyyy-mm-dd hh:mm:ss:ms";
            var insertLineDateText = "crlf() + chars('-', 29) + ' ' + dateTime() + ' ' + chars('-', 30) + crlf()";
            var doubleClickTitleAction = Properties.DoubleClickTitleActionEnum.None;

            InsertLineText = insertLineText;
            InsertDateText = insertDateText;
            InsertLineDateText = insertLineDateText;
            DoubleClickTitleAction = doubleClickTitleAction;

            Properties.Config.ConfigInstance.InsertLineText = insertLineText;
            Properties.Config.ConfigInstance.InsertDateText = insertDateText;
            Properties.Config.ConfigInstance.InsertLineDateText = insertLineDateText;
            Properties.Config.ConfigInstance.DoubleClickTitleAction = doubleClickTitleAction;
        }
        public SettingsViewModel()
        {
            isAutoStart = Properties.Config.ConfigInstance.RunOnStartUp;
            insertLineText = Properties.Config.ConfigInstance.InsertLineText;
            insertDateText = Properties.Config.ConfigInstance.InsertDateText;
            insertLineDateText = Properties.Config.ConfigInstance.InsertLineDateText;
            doubleClickTitleAction = Properties.Config.ConfigInstance.DoubleClickTitleAction;
        }
    }
}
