using ctxmgr.Page.Settings;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace ctxmgr.Properties
{
    public enum DoubleClickTitleActionEnum
    {
        None,
        EditTitle,
        DeletePage
    }
    public class Config
    {
        public static Config ConfigInstance = null!;
        private static readonly string SettingsPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Config.json"
        );
        public double WindowLeft { get; set; } = -1;
        public double WindowTop { get; set; } = -1;
        public double WindowWidth { get; set; } = -1;
        public double WindowHeight { get; set; } = -1;
        public int PageIndex { get; set; } = -1;
        public bool StayOnTop{ get; set; } = false;
        public bool RunOnStartUp { get; set; } = false;
        public bool TextWrap { get; set; } = false;
        public string InsertLineText { get; set; } = Properties.ConstVariables.INSERT_LINE_TEXT;
        public string InsertDateText { get; set; } = Properties.ConstVariables.INSERT_DATE_TEXT;
        public string InsertLineDateText { get; set; } = Properties.ConstVariables.INSERT_LINE_DATE_TEXT;
        public DoubleClickTitleActionEnum DoubleClickTitleAction { set; get; } = DoubleClickTitleActionEnum.None;
        public ObservableCollection<CustomerTextSnippet> CustomerTextSnippets { get; set; } = new ObservableCollection<CustomerTextSnippet>() {
        };
        public static Config Load()
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<Config>(json) ?? new Config();
            }
            return new Config();
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
    }
}
