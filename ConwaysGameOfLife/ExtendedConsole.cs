using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ExtendedWinConsole 
{
    public static class ExConsole // it is not recommended to use the normal System.Console class in combination with this class for IO 
    {
        public const short MinWidth = 15, MinHeight = 1;
        private static Logger _logger = new();
        private static Utility _utility;
        private static SMALL_RECT _writtenRegion = new(), _windowPos = new();
        private static SafeFileHandle _outputHandle, _inputHandle, _windowHandle;
        private static COORD _cursor = new(0, 0);
        public static COORD Cursor
        {
            get
            {
                return _cursor;
            }
            set
            {
                if (value.y >= 0 && value.y < _height && value.x >= 0 && value.x < _width)
                {
                    _cursor = value;
                }
                else
                {
                    throw new ArgumentException("cursor position was outside of bounce");
                }
            }
        }
        private static CHAR_INFO[] _outputBuffer;
        private static INPUT_RECORD[] _inputRecords = new INPUT_RECORD[5];
        public static CHAR_INFO[] OutputBuffer 
        {
            get 
            {
                return _outputBuffer; 
            }
            set 
            {
                if (_outputBuffer.Length == value.Length)
                {
                    _outputBuffer = value;
                }
                else
                {
                    throw new ArgumentException("buffer size can not be changed here, use SetBufferSize()");
                }
            }
        }
        private static int _width = 0, _height = 0;
        public static int Width { get { return _width; } }
        public static int Height { get { return _height; } }
        private static ushort _baseColor = 15;
        private static short _startingIndex = 0;
        public static ushort BasColor
        { get { return _baseColor; } set { if (value < 16) { _baseColor = value; } } }

#pragma warning disable 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ExConsole()
        {
            _utility = new(_width);
            //Console.WriteLine(NativeFunc.GetConsoleWindow());
            _windowHandle = NativeFunc.GetConsoleWindow();
            if (_windowHandle.IsInvalid)
            {
                _logger.addError("invalid windowHandle");
                throw new Exception("invalid windowHandle id: " + _windowHandle.DangerousGetHandle());
            }

            _outputHandle = NativeFunc.GetStdHandle(HandleType.output);
            _inputHandle = NativeFunc.GetStdHandle(HandleType.input);
            if (_outputHandle.IsInvalid)
            { 
                _logger.addError("invalid outputHandle");
                throw new Exception("invalid outputHandle");
            }
            if (_inputHandle.IsInvalid)
            {
                _logger.addError("invalid inputHandle");
                throw new Exception("invalid intputHandle");
            }
            SetBufferSize((short)Console.WindowWidth, (short)Console.WindowHeight);

            if (!NativeFunc.GetWindowRect(_windowHandle, ref _windowPos))
            {
                _logger.addError("in GetWindowRect: win32error: " + Marshal.GetLastWin32Error());
                throw new Exception("error while getting rect of window");
            }
            //SetCursorVisiblity(false);
            SetReadSize(31);
        }
        public static void SetMaximumBufferSize(short width, short heith)
        {
            CONSOLE_SCREEN_BUFFER_INFO_EX conscreenbufinex = new CONSOLE_SCREEN_BUFFER_INFO_EX();
            conscreenbufinex.cbSize = (uint)Marshal.SizeOf<CONSOLE_SCREEN_BUFFER_INFO_EX>();
            if (!NativeFunc.GetConsoleScreenBufferInfoEx(_outputHandle, ref conscreenbufinex))
            {
                throw new Exception("error while getting buffer info " + Marshal.GetLastWin32Error());
            }
            if ( heith <= 0)
            {
                throw new ArgumentException("height");
            }
            if (width <= 0 )
            {
                throw new ArgumentException("width");
            }

            conscreenbufinex.dwSize = new COORD(width, heith);
            if (!NativeFunc.SetConsoleScreenBufferInfoEx(_outputHandle, ref conscreenbufinex))
            {

                throw new Exception("error while setting buffer info " + Marshal.GetLastWin32Error());
            }
        }
        public static void SetCursorVisiblity(bool visible, int? cursorSize = null) // bug: sometimes the function fails when it is called befor any other 
        {
            CONSOLE_CURSOR_INFO CCI = new CONSOLE_CURSOR_INFO();
            if (!NativeFunc.GetConsoleCursorInfo(_outputHandle, out CCI))
            {
                _logger.addError("in SetCursorVisibility: error while getting: win32error: "+Marshal.GetLastWin32Error());
                throw new Exception(_logger.getLatest());
                //return false;
            }
            CCI.bVisible = visible;
            if (cursorSize.HasValue)
            {
                if (cursorSize.Value <= 0 && cursorSize.Value > 100)
                {
                    throw new ArgumentException("cursorSize must be between 1 an 100");
                }
                CCI.dwSize = (uint)cursorSize.Value;
            }
            if (!NativeFunc.SetConsoleCursorInfo(_outputHandle, ref CCI))
            {
                _logger.addError("in SetCursorVisibility: error while setting: win32error: " + Marshal.GetLastWin32Error());
                throw new Exception(_logger.getLatest());
                //return false;
            }
            //return true;

        }
        public static void SetWindowSize(int width, int height, bool valueIsInCharaters = false) // value in character = true isnt working properly
        {
            CONSOLE_FONT_INFOEX? CFIX = new CONSOLE_FONT_INFOEX?();
            CFIX = GetFont();
            if (CFIX == null)
            { 
                //return false;
                throw new Exception("getting error");
            }
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("value must be greater 0");
            }
            if (valueIsInCharaters)
            {
                if (!NativeFunc.MoveWindow(_windowHandle, _windowPos.Left, _windowPos.Top, width * CFIX.Value.dwFontSize.x + 1, height * CFIX.Value.dwFontSize.y + 2, true))
                {
                    _logger.addError("in ResizeWindow: in MoveWindow: " + Marshal.GetLastWin32Error());
                    //return false;
                    throw new ArgumentException(_logger.getLatest());
                }
            }
            else
            {
                if (!NativeFunc.MoveWindow(_windowHandle, _windowPos.Left, _windowPos.Top, width,  height, true))
                {
                    _logger.addError("in ResizeWindow: in MoveWindow: " + Marshal.GetLastWin32Error());
                    //return false;
                    throw new ArgumentException(_logger.getLatest());
                }
            }

            //return true;
        }
        public static void MoveWindowPos(int ofsetX, int ofsetY) // win32 error: 1400 (ERROR_INVALID_WINDOW_HANDLE)
        {
            try
            {
                if (!NativeFunc.MoveWindow(_windowHandle, _windowPos.Left + ofsetX, _windowPos.Top + ofsetY, _windowPos.Right, _windowPos.Bottom, true))
                {
                    _logger.addError("in MoveWindowPos: in MoveWindow: " + Marshal.GetLastWin32Error());
                    throw new ArgumentException(_logger.getLatest());
                }
            }
            catch (SEHException e)
            {
                _logger.addError("in moveWindowPos: win32 error: "+ Marshal.GetLastWin32Error().ToString()+" throw: "+e);
                throw new Exception("something with win32 and window position");
            }
        }
        public static void SetBufferSize(short sizeX, short sizeY) 
        {
            if (sizeX <  15 || sizeY <= 0) //maybe nood to chnage that in other funcs too
            {
                _logger.addError("in SetBufferSize: size is to small");
                throw new ArgumentException("size is to small: sizeX >=  16 && sizeY > 0");
            }
            _width = sizeX;
            _height = sizeY;
            _writtenRegion = new SMALL_RECT(0, 0, (short)_width, (short)_height);
            _outputBuffer = new CHAR_INFO[_width * _height];
#pragma warning restore

            _utility = new(_width);

            FlushBuffer();
            if (!NativeFunc.SetConsoleScreenBufferSize(_outputHandle, new COORD ((short)_width, (short)_height)))
            {
                throw new Exception(Marshal.GetLastWin32Error().ToString());
            }
        }
        public static void SetBufferSize(COORD size)
        {
           SetBufferSize(size.x, size.y);
        }
        public static void SetColor(int index, Color c)
        {
            SetColor(index, new COLORREF(c));
        }
        public static void SetColor(int index, COLORREF c) // (fixed) a bug where the window size decreases by 1 every time this gets called
        {
            CONSOLE_SCREEN_BUFFER_INFO_EX conscreenbufinex = new CONSOLE_SCREEN_BUFFER_INFO_EX();
            conscreenbufinex.cbSize = (uint)Marshal.SizeOf<CONSOLE_SCREEN_BUFFER_INFO_EX>();
            if (!NativeFunc.GetConsoleScreenBufferInfoEx(_outputHandle, ref conscreenbufinex))
            {

                throw new Exception("error while getting buffer info "+Marshal.GetLastWin32Error());
            }
            if (!(index >= 0 && index < 16))
                throw new ArgumentException("index must be between 0 and 15");
            conscreenbufinex.ColorTable[index] = c;
            conscreenbufinex.srWindow.Bottom++; // i have no clue why this line is fixing the bug
            if (!NativeFunc.SetConsoleScreenBufferInfoEx(_outputHandle, ref conscreenbufinex))
            {

                throw new Exception("error while setting buffer info " + Marshal.GetLastWin32Error());
            }
        }
        public static CONSOLE_FONT_INFOEX? GetFont()
        {
            CONSOLE_FONT_INFOEX confoninfex = new CONSOLE_FONT_INFOEX();
            confoninfex.cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>();
            if (!NativeFunc.GetCurrentConsoleFontEx(_outputHandle, false, ref confoninfex))
            {
                _logger.addInfo("getFont failed: win32error"+Marshal.GetLastWin32Error().ToString());
                return null;
            }
            return confoninfex;
        }
        public static void SetFont(int width, int height, string fontStyle = " ")
        {
            CONSOLE_FONT_INFOEX confoninfex = new CONSOLE_FONT_INFOEX();
            confoninfex.cbSize = (uint)Marshal.SizeOf<CONSOLE_FONT_INFOEX>();
            if (!NativeFunc.GetCurrentConsoleFontEx(_outputHandle, false, ref confoninfex))
            {

                throw new Exception("error while getting font info " + Marshal.GetLastWin32Error());
            }
            if (!(width > 1 && height > 1))
            {
                throw new ArgumentException("must be greater 1");
            }
            confoninfex.dwFontSize = new COORD((short)width, (short)height);
            if (fontStyle != " ")
                confoninfex.FaceName = fontStyle;
            if (!NativeFunc.SetCurrentConsoleFontEx(_outputHandle, false, ref confoninfex))
            {
                throw new Exception("error while setting font info " + Marshal.GetLastWin32Error());
            }

        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void UpdateBuffer(bool flushBuffer = true)
        {
            if (!NativeFunc.WriteConsoleOutput(_outputHandle, _outputBuffer, new COORD((short)_width, (short)_height), new COORD(0, 0), ref _writtenRegion))
            {
                string error = Marshal.GetLastWin32Error().ToString();
                _logger.addError($"lastWin32 error: {error}");
                _logger.addInfo(_writtenRegion.ToString());
                throw new Exception(_logger.getLatest());
            }
            if (flushBuffer)
            {
                FlushBuffer();
            }
        }
        public static void Clear(bool updateBuffer = true)
        {
            FlushBuffer();
            if (updateBuffer)
            {
                UpdateBuffer(false);
            }
        }
        private static void FlushBuffer(char baseChar = ' ')
        {
            _cursor.y = 0;
            _cursor.x = 0;
            for (int i = 0; i < _outputBuffer.Length; i++)
            {
                _outputBuffer[i].UnicodeChar = baseChar;
                _outputBuffer[i].Attributes = _baseColor; 
            }
        }
        public static void WriteLine(char c)
        {
            WriteLine(c.ToString());
        }
        public static void WriteLine(object obj)
        {
            WriteLine(obj.ToString());
        }
        public static void WriteLine(string text)
        {
            Write(text);
            _cursor.y++;
            _cursor.x = 0;
        }
        public static void WriteLine(char c, ushort color)
        {
            WriteLine(c.ToString(), color);
        }
        public static void WriteLine(object obj, ushort color)
        {
            string? text = obj.ToString();
            if (text != null)
                WriteLine(text, color);
        }
        public static void WriteLine(string text, ushort color)
        {
            Write(text, color);
            _cursor.y++;
            _cursor.x = 0;
        }
        public static void WriteLine()
        {
            _cursor.y++;
            _cursor.x = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization, MethodCodeType = MethodCodeType.IL)]
        public static void Write(string text, ushort color)
        {
            if (color > 15)
            {
                color = _baseColor;
            }
            COORD tempCursorPos = _cursor;
            int i = _utility.Convert2dTo1d(tempCursorPos.x, tempCursorPos.y);
            for (int j = 0; j < text.Length; i++, j++)
            {
                if (++tempCursorPos.x == _width)
                {
                    tempCursorPos.x = _startingIndex;
                    tempCursorPos.y++;
                    i = _utility.Convert2dTo1d(tempCursorPos.x, tempCursorPos.y);
                }
                if (text[j] == '\n')
                {
                    tempCursorPos.y++;
                    tempCursorPos.x = _startingIndex;
                    i = _utility.Convert2dTo1d(tempCursorPos.x, tempCursorPos.y); // possible out of bounce?
                    if (i >= _width * _height)
                    {
                        i = (_width * _height) - 1;
                        break;
                    }
                    if (++j == text.Length) //out of bounce check IF \n is the last char in the string
                    {
                        continue;
                    }
                }
                _outputBuffer[i].UnicodeChar = text[j];
                _outputBuffer[i].Attributes = color;
            }
            _cursor = tempCursorPos;
            UpdateBuffer(false);
        }
        public static void Write(object obj, ushort color)
        {
            string? text = obj.ToString();
            if (text != null)
                Write(text, color);
        }
        public static void Write(char c)
        {
            Write(c.ToString());
        }
        public static void Write(char c, ushort color)
        {
            Write(c.ToString(), color);
        }
        public static void Write(object obj)
        {
            Write(obj.ToString());
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void Write(string text)
        {
            COORD tempCursorPos = _cursor;
            int i = _utility.Convert2dTo1d(tempCursorPos.x, tempCursorPos.y);
            for (int j = 0; j < text.Length && i < _outputBuffer.Length; i++, j++)
            {
                if (++tempCursorPos.x == _width)
                {
                    tempCursorPos.x = _startingIndex;
                    tempCursorPos.y++;
                    i = _utility.Convert2dTo1d(tempCursorPos.x, tempCursorPos.y);
                }
                if (text[j] == '\n')
                {
                    tempCursorPos.y++;
                    tempCursorPos.x = 0;
                    i = _utility.Convert2dTo1d(tempCursorPos.x, tempCursorPos.y); // possible out of bounce?
                    if (i >= _width*_height)
                    {
                        i = (_width*_height)-1;
                        break;
                    }
                    if (++j == text.Length) //out of bounce check
                    {
                        continue;
                    }
                }
                _outputBuffer[i].UnicodeChar = text[j];
                _outputBuffer[i].Attributes = _baseColor;
            }
            _cursor = tempCursorPos;
            UpdateBuffer(false);
        }
        public static void Remove(bool skipSpaceChar = true, bool updateBuffer = true)
        {
            if (!skipSpaceChar)
            {
                if (--_cursor.x < 0)
                {
                    _cursor.x = (short)(_width - 1);
                    if (--_cursor.y < 0)
                    {
                        _cursor.y = 0;
                    }
                }
            }
            else
            {
                while (_outputBuffer[_utility.Convert2dTo1d(_cursor.x, _cursor.y)].UnicodeChar == ' ')
                {
                    if (--_cursor.x < 0)
                    {
                        _cursor.x = (short)(_width - 1);
                        if (--_cursor.y < 0)
                        {
                            _cursor.y = 0;
                        }
                    }
                } 
            }
            _outputBuffer[_utility.Convert2dTo1d(_cursor.x, _cursor.y)].UnicodeChar = ' ';
            _outputBuffer[_utility.Convert2dTo1d(_cursor.x, _cursor.y)].Attributes = _baseColor;
            if (updateBuffer)
            {
                UpdateBuffer(false);
            }
        }
        private static void SetReadSize(uint size)
        {
            _inputRecords = new INPUT_RECORD[size];
        }
        public static string ReadLine(bool displayInput = true)
        {
            uint numberOfEventsRead = 0; 
            List<char> textBuffer = new();
            if (!NativeFunc.FlushConsoleInputBuffer(_inputHandle))
            {
                throw new Exception("win32 error: "+Marshal.GetLastWin32Error());
            }
            while (true)
            {
                if (!NativeFunc.ReadConsoleInput(_inputHandle, _inputRecords, (uint)_inputRecords.Length, out numberOfEventsRead))
                {
                    throw new Exception("win32error: " + Marshal.GetLastWin32Error());
                }
                for (int i = 0; i < numberOfEventsRead; i++)
                {
                    if (_inputRecords[i].EventType == (ushort)InputEventType.KEY_EVENT && _inputRecords[i].Event.KeyEvent.bKeyDown == true) //input buffer one key event for key up and key down 
                    {
                        //Write($"{_inputRecords[i].Event.KeyEvent.dwControlKeyState:x}");
                        if (textBuffer.Count > 0 && _inputRecords[i].Event.KeyEvent.UnicodeChar == '\b') 
                        {
                            if (displayInput)
                            {
                                Remove();
                            }
                            textBuffer.RemoveAt(textBuffer.Count - 1);
                        }
                        else if (_inputRecords[i].Event.KeyEvent.UnicodeChar == '\u000d') // 0x0D is ENTER Key
                        {
                            goto ReadLineEnd;
                        } 
                        else if (_inputRecords[i].Event.KeyEvent.UnicodeChar == '\0'||_inputRecords[i].Event.KeyEvent.UnicodeChar == '\u0016' || _inputRecords[i].Event.KeyEvent.UnicodeChar == '\b')  // \0 lands in the input buffer, if something like  a lot and we dont want it here 
                        {
                            continue;
                        }
                        else
                        {
                            if (displayInput)
                            {
                                Write(_inputRecords[i].Event.KeyEvent.UnicodeChar);
                            }
                            textBuffer.Add(_inputRecords[i].Event.KeyEvent.UnicodeChar);
                        }
                    }
                }
            }
        ReadLineEnd:
            string output = new string(textBuffer.ToArray());
            WriteLine();
            return output;
        }
        public static ConsoleKeyInfo ReadKey(bool displayInput = false) // not really tested maigh be very bug
        {
            uint numberOfEventsRead = 0;
            if (!NativeFunc.FlushConsoleInputBuffer(_inputHandle))
            {
                throw new Exception("win32 error: " + Marshal.GetLastWin32Error());
            }
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
            while (true)
            {
                if (!NativeFunc.ReadConsoleInput(_inputHandle, _inputRecords, (uint)_inputRecords.Length, out numberOfEventsRead))
                {
                    throw new Exception("win32error: " + Marshal.GetLastWin32Error());
                }
                if (_inputRecords[0].EventType == (ushort)InputEventType.KEY_EVENT && _inputRecords[0].Event.KeyEvent.bKeyDown == true) //input buffer one key event for key up and key down 
                {
                    if (displayInput)
                    {
                        Write(_inputRecords[0].Event.KeyEvent.UnicodeChar);
                    }
                    bool shift = false, alt = false, control = false;
                    switch (_inputRecords[0].Event.KeyEvent.dwControlKeyState)
                    {
                        case ControlKeyState.SHIFT_PRESSED:
                            shift = true;
                            break;
                        case ControlKeyState.LEFT_CTRL_PRESSED:
                        case ControlKeyState.RIGHT_CTRL_PRESSED:
                            control = true;
                            break;
                        case ControlKeyState.LEFT_ALT_PRESSED:
                        case ControlKeyState.RIGHT_ALT_PRESSED:
                            alt = true;
                            break;
                        default:
                            break;
                    }
                    keyInfo = new ConsoleKeyInfo(_inputRecords[0].Event.KeyEvent.UnicodeChar, (ConsoleKey)_inputRecords[0].Event.KeyEvent.wVirtualScanCode, shift, alt, control);
                    goto ReadKeyEnd;
                }

            }
        ReadKeyEnd:
            
            //WriteLine();
            return keyInfo;
        }
        public static int Get2dBufferIndex(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
                return _utility.Convert2dTo1d(x, y);
            else
                throw new IndexOutOfRangeException("x/y must be below width/heigh and above");
        }
        public static void WriteSubWindow(SubWindow sw) 
        {
            for (int y = 0; y < sw.rect.Bottom && y + sw.rect.Top < _height; y++)
            {
                for (int x = 0; x < sw.rect.Right && x + sw.rect.Left < _width; x++)
                {
                    _outputBuffer[_utility.Convert2dTo1d(x + sw.rect.Left, y + sw.rect.Top)] = sw.buffer[sw.Utility.Convert2dTo1d(x, y)]; 
                }
            }
        }
        public static void ClearSubWindow(SubWindow sw)
        {
            CHAR_INFO ci = new();
            ci.Attributes = _baseColor;
            ci.UnicodeChar = ' ';
            for (int y = 0; y < sw.rect.Bottom && y + sw.rect.Top < _height; y++)
            {
                for (int x = 0; x < sw.rect.Right && x + sw.rect.Left < _width; x++)
                {
                    _outputBuffer[_utility.Convert2dTo1d(x + sw.rect.Left, y + sw.rect.Top)] = ci;
                }
            }
        }
        public static string[] GetLogs()
        {
            return _logger.getAll();
        }
    }
}