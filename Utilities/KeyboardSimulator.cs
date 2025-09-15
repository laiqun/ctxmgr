using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



namespace ctxmgr.Utilities
{
    public static class KeyboardSimulator
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        private const byte VK_SHIFT = 0x10;
        private const byte VK_CONTROL = (byte)0x11;
        private const byte VK_Alt = (byte)0x12;
        private const byte VK_WIN_LEFT = (byte)0x5B;
        //private const byte VK_WIN_RIGHT = (byte)RWin;
        private const byte VK_C = 0x43;
        
        public static void SendCtrlC()
        {
            // Ctrl down
            keybd_event(VK_CONTROL, 0, 0, 0);
            // C down
            keybd_event(VK_C, 0, 0, 0);
            // C up
            keybd_event(VK_C, 0, 2, 0);
            // Ctrl up
            keybd_event(VK_CONTROL, 0, 2, 0);
        }
       
    }
}
