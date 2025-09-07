using CommunityToolkit.Mvvm.ComponentModel;
using ctxmgr.Page.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ctxmgr
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<CustomerTextSnippet> CustomerTextSnippets => Properties.Config.ConfigInstance.CustomerTextSnippets;
        
    }
}
