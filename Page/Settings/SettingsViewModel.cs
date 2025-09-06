using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ctxmgr.Properties;
using ctxmgr.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddNewTextSnippetCommand))]
        private string newTextSnippetTxtBox;

        private bool CanExecuteAddNewTextSnippet()
        {
            if (string.IsNullOrWhiteSpace(NewTextSnippetTxtBox)) return false;
            if (CustomerTextSnippets.Any(x => x.Text == NewTextSnippetTxtBox))
            {
                return false;
            }
            return true;
        }
        [RelayCommand(CanExecute =nameof(CanExecuteAddNewTextSnippet))]
        private void AddNewTextSnippet()
        {
            var newTxtSnippet = new CustomerTextSnippet(NewTextSnippetTxtBox);
            CustomerTextSnippets.Add(newTxtSnippet);
            NewTextSnippetTxtBox = string.Empty;
            
            Properties.Config.ConfigInstance.Save();
        }



        [RelayCommand]
        private void DeleteTextSnippet(CustomerTextSnippet snippet)
        {
            if (CustomerTextSnippets.Contains(snippet))
            {
                CustomerTextSnippets.Remove(snippet);
                Properties.Config.ConfigInstance.Save();
            }
            
        }

        public ObservableCollection<CustomerTextSnippet> CustomerTextSnippets => Properties.Config.ConfigInstance.CustomerTextSnippets;


        public void ResetToDefault()
        {
            var insertLineText = Properties.ConstVariables.INSERT_LINE_TEXT;
            var insertDateText = Properties.ConstVariables.INSERT_DATE_TEXT;
            var insertLineDateText = Properties.ConstVariables.INSERT_LINE_DATE_TEXT;
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
        private bool isLoading = true;
        public SettingsViewModel()
        {
            isLoading = true;
            isAutoStart = Properties.Config.ConfigInstance.RunOnStartUp;
            insertLineText = Properties.Config.ConfigInstance.InsertLineText;
            insertDateText = Properties.Config.ConfigInstance.InsertDateText;
            insertLineDateText = Properties.Config.ConfigInstance.InsertLineDateText;
            doubleClickTitleAction = Properties.Config.ConfigInstance.DoubleClickTitleAction;
            foreach(var item in CustomerTextSnippets)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                item.PropertyChanged += Item_PropertyChanged;
            }
            CustomerTextSnippets.CollectionChanged += CustomerTextSnippets_CollectionChanged;
            isLoading = false;
        }

        private void CustomerTextSnippets_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CustomerTextSnippet item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (CustomerTextSnippet item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;
            }
        }
        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (isLoading) return;
            Properties.Config.ConfigInstance.Save();
        }
    }
    public partial class CustomerTextSnippet : ObservableObject
    {
        [ObservableProperty]
        private string text;

        public CustomerTextSnippet(string text) {
            Text = text;
            
        }
    }
}
