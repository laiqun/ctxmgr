using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;

namespace ctxmgr
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
       protected override void OnStartup(StartupEventArgs e)
        {
            CultureInfo current = Thread.CurrentThread.CurrentUICulture;
            if(current.TwoLetterISOLanguageName == "zh")
                ctxmgr.Properties.Resources.Culture = new CultureInfo("zh");
            base.OnStartup(e);
        }
    }

}
