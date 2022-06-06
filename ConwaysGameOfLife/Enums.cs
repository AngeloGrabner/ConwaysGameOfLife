using System;

namespace ExtendedWinConsole
{
    public enum BorderType : int
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3,
        VerticalLine = 4,
        HoriziontalLine = 5
        // to be added intersecting borders
    }
    [Flags]
    public enum ControlKeyState
    {
        RIGHT_ALT_PRESSED = 0x1,
        LEFT_ALT_PRESSED = 0x2,
        RIGHT_CTRL_PRESSED = 0x4,
        LEFT_CTRL_PRESSED = 0x8,
        SHIFT_PRESSED = 0x10,
        NUMLOCK_ON = 0x20,
        SCROLLLOCK_ON = 0x40,
        CAPSLOCK_ON = 0x80,
        ENHANCED_KEY = 0x100
    }
    public enum InputEventType : ushort
    {
        FOCUS_EVENT = 0x0010,
        KEY_EVENT = 0x0001,
        MENU_EVENT = 0x0008,
        MOUSE_EVENT = 0x0002,
        WINDOW_BUFFER_SIZE_EVENT = 0x0004
    }
    public enum HandleType
    {
        error = -12,
        output = -11,
        input = -10
    }
}
