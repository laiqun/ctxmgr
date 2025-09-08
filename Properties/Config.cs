using CommunityToolkit.Mvvm.ComponentModel;
using ctxmgr.Page.Settings;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace ctxmgr.Properties
{
    public enum DoubleClickTitleActionEnum
    {
        None,
        EditTitle,
        DeletePage
    }
    public enum ThemeMode { 
        Dark,
        Light
    }
    public partial  class  FontSetting:ObservableObject
    {
        [ObservableProperty]
        private double fontSize = 12;

        [ObservableProperty]
        private bool isBold = false;
        [ObservableProperty]
        private bool isItalic = false;
        [ObservableProperty]
        private bool isUnderline = false;
        [ObservableProperty]
        private string fontFamily = "Microsoft YaHei UI";


        [JsonIgnore]
        public byte Alpha
        {
            get => (byte)((FontColor & 0xFF000000) >> 24);
            set
            {
                FontColor = (FontColor & 0x00FFFFFF) | ((uint)value << 24);
                OnPropertyChanged(nameof(FontColor)); // 通知
                OnPropertyChanged(nameof(SelectedColor)); // 通知
            }
        }

        [JsonIgnore]
        public byte Red
        {
            get => (byte)((FontColor & 0x00FF0000) >> 16);
            set
            {
                FontColor = (FontColor & 0xFF00FFFF) | ((uint)value << 16);
                OnPropertyChanged(nameof(FontColor)); // 通知
                OnPropertyChanged(nameof(SelectedColor)); // 通知
            }
        }

        [JsonIgnore]
        public byte Green
        {
            get => (byte)((FontColor & 0x0000FF00) >> 8);
            set
            {
                FontColor = (FontColor & 0xFFFF00FF) | ((uint)value << 8);
                OnPropertyChanged(nameof(FontColor)); // 通知
                OnPropertyChanged(nameof(SelectedColor)); // 通知
            }
        }

        [JsonIgnore]
        public byte Blue
        {
            get => (byte)(FontColor & 0x000000FF);
            set
            {
                FontColor = (FontColor & 0xFFFFFF00) | value;
                OnPropertyChanged(nameof(FontColor)); // 通知
                OnPropertyChanged(nameof(SelectedColor)); // 通知
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Alpha))]
        [NotifyPropertyChangedFor(nameof(Red))]
        [NotifyPropertyChangedFor(nameof(Green))]
        [NotifyPropertyChangedFor(nameof(Blue))]
        private long fontColor = 0xffffffff;
        [JsonIgnore]
        public  Brush SelectedColor => new SolidColorBrush(Color.FromArgb(Alpha,Red,Green,Blue));

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
        public ThemeMode Theme { get; set; } = ThemeMode.Dark;
        public long BackgroundColor { get; set; } = 0x00000000;
        public FontSetting Font { get; set; } = new FontSetting();
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
