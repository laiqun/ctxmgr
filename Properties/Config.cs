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

    public partial  class  StyleSetting:ObservableObject
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
        [NotifyPropertyChangedFor(nameof(SelectedColor))]
        private long fontColor = 0xffffffff;



        [JsonIgnore]
        public byte BgAlpha
        {
            get => (byte)((BackgroundColor & 0xFF000000) >> 24);
            set
            {
                BackgroundColor = (BackgroundColor & 0x00FFFFFF) | ((uint)value << 24);
                OnPropertyChanged(nameof(BackgroundColor)); // 通知
                OnPropertyChanged(nameof(BgSelectedColor)); // 通知
            }
        }

        [JsonIgnore]
        public byte BgRed
        {
            get => (byte)((BackgroundColor & 0x00FF0000) >> 16);
            set
            {
                BackgroundColor = (BackgroundColor & 0xFF00FFFF) | ((uint)value << 16);
                OnPropertyChanged(nameof(BackgroundColor)); // 通知
                OnPropertyChanged(nameof(BgSelectedColor)); // 通知
            }
        }

        [JsonIgnore]
        public byte BgGreen
        {
            get => (byte)((BackgroundColor & 0x0000FF00) >> 8);
            set
            {
                BackgroundColor = (BackgroundColor & 0xFFFF00FF) | ((uint)value << 8);
                OnPropertyChanged(nameof(BackgroundColor)); // 通知
                OnPropertyChanged(nameof(BgSelectedColor)); // 通知
            }
        }

        [JsonIgnore]
        public byte BgBlue
        {
            get => (byte)(BackgroundColor & 0x000000FF);
            set
            {
                BackgroundColor = (BackgroundColor & 0xFFFFFF00) | value;
                OnPropertyChanged(nameof(BackgroundColor)); // 通知
                OnPropertyChanged(nameof(BgSelectedColor)); // 通知
            }
        }


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BgAlpha))]
        [NotifyPropertyChangedFor(nameof(BgRed))]
        [NotifyPropertyChangedFor(nameof(BgGreen))]
        [NotifyPropertyChangedFor(nameof(BgBlue))]
        [NotifyPropertyChangedFor(nameof(BgSelectedColor))]
        private long backgroundColor = 0xff000000;

        [JsonIgnore]
        public  Brush SelectedColor => new SolidColorBrush(Color.FromArgb(Alpha,Red,Green,Blue));

        [JsonIgnore]
        public Brush BgSelectedColor => new SolidColorBrush(Color.FromArgb(BgAlpha, BgRed, BgGreen, BgBlue));

        public void NotifyBgColorChanged() {
            OnPropertyChanged(nameof(BgSelectedColor));
        }
    }
    public class Config
    {
        public static Config ConfigInstance = null!;

        private static string SettingsPath
        {
            get {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                string exeFolder = System.IO.Path.GetDirectoryName(exePath);
                return Path.Combine(
                exeFolder,
                "data", "config.json");
            }
        }
        public double WindowLeft { get; set; } = -1;
        public double WindowTop { get; set; } = -1;
        public double WindowWidth { get; set; } = -1;
        public double WindowHeight { get; set; } = -1;
        public int PageIndex { get; set; } = -1;
        public bool StayOnTop{ get; set; } = false;
        //44-69 A-Z
        public int HotKeyBase { get; set; } = 69;// System.Windows.Input.Key.Z
        // 0: None, 1: Alt, 2: Ctrl, 4: Shift, 8: Win
        public int HotKeyModifiers { get; set; } = 1;
        
        public bool RunOnStartUp { get; set; } = false;
        public int LastCaretIndex { get; set; } = -1;
        public string LastPage { get; set; } = string.Empty;
        public StyleSetting Style { get; set; } = new StyleSetting();
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
