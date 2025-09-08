using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ctxmgr.Page.BgColor
{
    public class BackgroundColorWindowViewModel : ObservableObject
    {
        private long FontColor {
            get
            {
                return ctxmgr.Properties.Config.ConfigInstance.Style.BackgroundColor;
            }
            set {
                ctxmgr.Properties.Config.ConfigInstance.Style.BackgroundColor = value;
                ctxmgr.Properties.Config.ConfigInstance.Style.NotifyBgColorChanged();
            }
        }
        public byte Alpha
        {
            get => (byte)((FontColor & 0xFF000000) >> 24);
            set
            {
                FontColor = (FontColor & 0x00FFFFFF) | ((uint)value << 24);
                OnPropertyChanged(nameof(SelectedColor)); // 通知
            }
        }


        public byte Red
        {
            get => (byte)((FontColor & 0x00FF0000) >> 16);
            set
            {
                FontColor = (FontColor & 0xFF00FFFF) | ((uint)value << 16);
                OnPropertyChanged(nameof(SelectedColor)); // 通知
            }
        }


        public byte Green
        {
            get => (byte)((FontColor & 0x0000FF00) >> 8);
            set
            {
                FontColor = (FontColor & 0xFFFF00FF) | ((uint)value << 8);
                OnPropertyChanged(nameof(SelectedColor)); // 通知
            }
        }


        public byte Blue
        {
            get => (byte)(FontColor & 0x000000FF);
            set
            {
                FontColor = (FontColor & 0xFFFFFF00) | value;
                OnPropertyChanged(nameof(SelectedColor)); // 通知
            }
        }
        public Brush SelectedColor => new SolidColorBrush(Color.FromArgb(Alpha, Red, Green, Blue));
    }
}
