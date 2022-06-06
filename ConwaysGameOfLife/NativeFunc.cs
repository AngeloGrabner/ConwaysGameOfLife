using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace ExtendedWinConsole
{
    public class NativeFunc
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle GetStdHandle(HandleType nStdHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool WriteConsoleOutput(
            SafeFileHandle hConsoleOutput,
            CHAR_INFO[] lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpWriteRegion);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern COORD GetLargestConsoleWindowSize(SafeFileHandle hConsoleOutput);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetCurrentConsoleFontEx(SafeFileHandle hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetCurrentConsoleFontEx(SafeFileHandle hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleScreenBufferInfoEx(SafeFileHandle hConsoleOutput, ref CONSOLE_SCREEN_BUFFER_INFO_EX lpConsoleScreenBufferInfo);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferInfoEx(SafeFileHandle hConsoleOuput, ref CONSOLE_SCREEN_BUFFER_INFO_EX lpConsoleScreenBufferInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(SafeFileHandle hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
        internal static extern bool GetWindowRect(SafeFileHandle hWnd, ref SMALL_RECT rect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferSize(SafeFileHandle hConsoleOutput, COORD dwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorInfo(
            SafeFileHandle hConsoleOutput,
            [In] ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleCursorInfo(
            SafeFileHandle hConsoleOutput,
            out CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool ReadConsoleInput( //https://docs.microsoft.com/en-us/windows/console/readconsoleinput
            SafeFileHandle hConsoleInput,
            [Out] INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead
        );
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FlushConsoleInputBuffer(SafeFileHandle hConsoleInput);
    }

}
