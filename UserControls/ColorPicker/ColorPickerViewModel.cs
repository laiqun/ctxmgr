using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ctxmgr.UserControls.ColorPicker
{
    public partial class ColorPickerViewModel:ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedColor))]
        private byte a;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedColor))]
        private byte r;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedColor))]
        private byte g;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedColor))]
        private byte b;

        public SolidColorBrush SelectedColor => new SolidColorBrush(Color.FromArgb(A, R, G, B));
        public ColorPickerViewModel() {
            a = 0;
            r = 0;
            g = 0;
            b = 0;
        }
    }
}
