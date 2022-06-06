using System.Runtime.InteropServices;
using System.Drawing;
using System;

namespace ExtendedWinConsole
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SMALL_RECT
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
        public SMALL_RECT(short left, short top, short right, short bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
        public override string ToString()
        {
            return $"Ledt: {Left}, Top: {Top}, Right {Right}, Bottom: {Bottom}";
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct COORD
    {
        public short x;
        public short y;

        public COORD(short x, short y)
        {
            this.x = x;
            this.y = y;
        }
    }
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct CHAR_INFO
    {
        [FieldOffset(0)]
        public char UnicodeChar;
        [FieldOffset(0)]
        public char AsciiChar;
        [FieldOffset(2)] //2 bytes seems to work properly
        public UInt16 Attributes;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_CURSOR_INFO
    {
        public uint dwSize;
        public bool bVisible;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CONSOLE_FONT_INFOEX
    {
        public CONSOLE_FONT_INFOEX(int VOID = 0)
        {
            cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>();
            nFont = 0;
            dwFontSize = new(0, 0);
            FontFamily = 0;
            FontWeight = 0;
            FaceName = String.Empty;
        }
        public uint cbSize;
        public uint nFont;
        public COORD dwFontSize;
        public int FontFamily;
        public int FontWeight;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_SCREEN_BUFFER_INFO_EX
    {
        public CONSOLE_SCREEN_BUFFER_INFO_EX(int VOID = 0)
        {
            cbSize= (uint)Marshal.SizeOf<CONSOLE_SCREEN_BUFFER_INFO_EX>();
            dwSize = new COORD(0,0);
            dwCursorPosition = new COORD(0,0);
            wAttributes = 0;
            srWindow = new SMALL_RECT(0,0,0,0);
            dwMaximumWindowSize = new COORD(0,0);

            wPopupAttributes = 0;
            bFullscreenSupported = false;
            ColorTable = new COLORREF[16];
        }
        public uint cbSize;
        public COORD dwSize;
        public COORD dwCursorPosition;
        public short wAttributes;
        public SMALL_RECT srWindow;
        public COORD dwMaximumWindowSize;

        public ushort wPopupAttributes;
        public bool bFullscreenSupported;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public COLORREF[] ColorTable ;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct COLORREF
    {
        internal uint ColorDWORD;

        internal COLORREF(ConsoleColor color)
        {
            ColorDWORD = (uint)color;
        }
        internal COLORREF(Color color)
        {
            ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
        }

        internal COLORREF(uint r, uint g, uint b)
        {
            ColorDWORD = r + (g << 8) + (b << 16);
        }

        internal Color GetColor()
        {
            return Color.FromArgb((int)(0x000000FFU & ColorDWORD),
               (int)(0x0000FF00U & ColorDWORD) >> 8, (int)(0x00FF0000U & ColorDWORD) >> 16);
        }

        internal void SetColor(Color color)
        {
            ColorDWORD = (uint)color.R + (((uint)color.G) << 8) + (((uint)color.B) << 16);
        }
    }

    // for console input 



    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT_RECORD
    {
        public ushort EventType;
        public INPUT_RECORD_UNION Event;
    };

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT_RECORD_UNION
    {
        [FieldOffset(0)]
        public KEY_EVENT_RECORD KeyEvent;
        [FieldOffset(0)]
        public MOUSE_EVENT_RECORD MouseEvent;
        [FieldOffset(0)]
        public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
        [FieldOffset(0)]
        public MENU_EVENT_RECORD MenuEvent;
        [FieldOffset(0)]
        public FOCUS_EVENT_RECORD FocusEvent;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct FOCUS_EVENT_RECORD
    {
        public bool bSetFocus;
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct KEY_EVENT_RECORD
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.Bool)]
        public bool bKeyDown;
        [FieldOffset(4), MarshalAs(UnmanagedType.U2)]
        public ushort wRepeatCount;
        [FieldOffset(6), MarshalAs(UnmanagedType.U2)]
        public ushort wVirtualKeyCode;
        [FieldOffset(8), MarshalAs(UnmanagedType.U2)]
        public ushort wVirtualScanCode;
        [FieldOffset(10)]
        public char UnicodeChar;
        [FieldOffset(10)]
        public byte AsciiChar;
        [FieldOffset(12), MarshalAs(UnmanagedType.U4)]
        public ControlKeyState dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENU_EVENT_RECORD
    {
        uint dwConmmanId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSE_EVENT_RECORD
    {
        COORD dwMousePosition;
        ushort dwButtonState;
        ushort dwControlKeyState;
        ushort dwEventFlags;
    }

    [StructLayout(LayoutKind.Sequential)]

    public struct WINDOW_BUFFER_SIZE_RECORD
    {
        COORD dwSize;
    }
}
