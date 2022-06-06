using System;
using System.Runtime.CompilerServices;

namespace ExtendedWinConsole
{
    public class SubWindow 
    {
        private char[] _borderTiles = new char[6] { '╔', '╗', '╚', '╝', '║', '═' };
        private ushort _baseColor = 15;
        private const short _startingIndex = 1;
        private COORD _cursor = new COORD(_startingIndex,_startingIndex);
        public COORD Cursor { get { return _cursor; } }
        private SMALL_RECT _rect;
        private Utility _utility;
        internal Utility Utility
        {
            get { return _utility; }
        }
        public SMALL_RECT rect
        {
            get { return _rect; }
            private set { _rect = value; }
        }
        private CHAR_INFO[] _buffer;
        public CHAR_INFO[] buffer
        {
            get { return _buffer; }
            private set { _buffer = value; }
        }
        public SubWindow(short x, short y, short width, short height)
        {
            _rect = new SMALL_RECT(x, y, width, height);
            _buffer = new CHAR_INFO[(width) * (height)];
            _utility = new(width);
            FillBuffer();
            DrawBorder();
        }
        public SubWindow(SMALL_RECT rect)
        {
            _rect = rect;
            _buffer = new CHAR_INFO[(_rect.Left - _rect.Right) * (_rect.Top - _rect.Bottom)];
            _utility = new(rect.Right);
            FillBuffer();
            DrawBorder();
        }
        public void WriteLine(string text)
        {
            Write(text);
            _cursor.x = _startingIndex;
            _cursor.y++;
        }
        public void WriteLine(object obj)
        {
            Write(obj);
            _cursor.x = _startingIndex;
            _cursor.y++;
        }
        public void WriteLine(string text, ushort color)
        {
            Write(text, color);
            _cursor.x = _startingIndex;
            _cursor.y++;
        }
        public void WriteLine(object obj, ushort color)
        {
            Write(obj, color);
            _cursor.x = _startingIndex;
            _cursor.y++;
        }
        public void Write(object obj, ushort color)
        {
            Write(obj.ToString(), color);
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization, MethodCodeType = MethodCodeType.IL)]
        public void Write(string text, ushort color)
        {
            if (color > 15)
            {
                color = 15;
            }
            COORD tempCursorPos = _cursor;
            int i = _utility.Convert2dTo1d(tempCursorPos.x, tempCursorPos.y);
            int end = _buffer.Length-(rect.Right+1);
            for (int j = 0; j < text.Length && i < end; i++, j++)
            {
                if (++tempCursorPos.x == rect.Right - 1)
                {
                    tempCursorPos.x = _startingIndex;
                    tempCursorPos.y++;
                    i = _utility.Convert2dTo1d(tempCursorPos.x, tempCursorPos.y);
                }
                if (text[j] == '\n')
                {
                    tempCursorPos.y++;
                    tempCursorPos.x = _startingIndex;
                    i = _utility.Convert2dTo1d(tempCursorPos.x, tempCursorPos.y);
                    if (++j == text.Length) 
                        break;
                }

                _buffer[i].UnicodeChar = text[j];
                _buffer[i].Attributes = color;
            }
            _cursor = tempCursorPos;
        }
        public void Write(object obj)
        {
            Write(obj.ToString());
        }
        public void Write(string text)
        {
            Write(text, _baseColor);
        }
        public void DrawBorder()
        {
            DrawBorder(15);
        }
        public void DrawBorder(ushort color)
        {
            if (color > 15)
            {
                color = 15;
            }
            for (int i = 1; i < _rect.Right-1; i++) // top line
            {
                _buffer[i].UnicodeChar = _borderTiles[(int)BorderType.HoriziontalLine];
                _buffer[i].Attributes = color;
            }
            for (int i = _rect.Right * _rect.Bottom -_rect.Right; i < _buffer.Length; i++) // bottom line
            {
                _buffer[i].UnicodeChar = _borderTiles[(int)BorderType.HoriziontalLine];
                _buffer[i].Attributes = color;
            }
            for (int i = rect.Right; i < _rect.Right*_rect.Bottom-1; i+= _rect.Right) // left line
            {
                _buffer[i].UnicodeChar = _borderTiles[(int)BorderType.VerticalLine];
                _buffer[i].Attributes = color;
            }
            for (int i = _rect.Right*2-1; i < _buffer.Length; i+= _rect.Right) // right line
            {
                _buffer[i].UnicodeChar = _borderTiles[(int)BorderType.VerticalLine];
                _buffer[i].Attributes = color;
            }
            // corners
            _buffer[0].UnicodeChar = _borderTiles[(int)BorderType.TopLeft];
            _buffer[0].Attributes = color;

            _buffer[_rect.Right-1].UnicodeChar = _borderTiles[(int)BorderType.TopRight];
            _buffer[_rect.Right-1].Attributes = color;

            _buffer[_utility.Convert2dTo1d(0, _rect.Bottom-1)].UnicodeChar = _borderTiles[(int)BorderType.BottomLeft]; // -1 could be wrong
            _buffer[_utility.Convert2dTo1d(0, _rect.Bottom-1)].Attributes = color;

            _buffer[(_rect.Bottom * _rect.Right) - 1].UnicodeChar = _borderTiles[(int)BorderType.BottomRight];
            _buffer[(_rect.Bottom * _rect.Right) - 1].Attributes = color;
        }
        public void Clear()
        {
            _cursor = new(1, 1);
            FillBuffer();
            DrawBorder();
        }
        public void Remove()
        {
            while (_buffer[_utility.Convert2dTo1d(_cursor.x, _cursor.y)].UnicodeChar == ' ')
            {
                if (--_cursor.x < 0)
                {
                    _cursor.x = (short)(_rect.Right - 1);
                    if (--_cursor.y < 0)
                    {
                        _cursor.y = 0;
                    }
                }
            }
            _buffer[_utility.Convert2dTo1d(_cursor.x, _cursor.y)].UnicodeChar = ' ';
            _buffer[_utility.Convert2dTo1d(_cursor.x, _cursor.y)].Attributes = _baseColor;
        }
        public void Resize(short newX, short newY,short newWidth, short newHeight)
        {
            _rect = new SMALL_RECT(newX,newY,newWidth,newHeight);
            _buffer = new CHAR_INFO[(_rect.Right) * (_rect.Bottom)];
            _utility = new(newWidth);
            FillBuffer();
            DrawBorder();
        }
        public void Resize(short newWidth, short newHeight)
        {
            Resize(_rect.Left, _rect.Top, newWidth, newHeight);
        }
        public void Move(short Xoffset, short Yoffset)
        {
            _rect.Left += Xoffset;
            _rect.Top += Yoffset;
        }
        private void FillBuffer(char defaultChar = ' ', ushort defaultColor = 15) 
        {
            if (defaultColor > 15)
            {
                throw new ArgumentException("unknown color number");
            }
            for (int i = 0; i < _buffer.Length; i++)
            {
                _buffer[i].UnicodeChar = defaultChar;
                _buffer[i].Attributes = defaultColor;
            }
        }
        public void SetBorder(BorderType BT, char c)
        {
            _borderTiles[(int)BT] = c;
        }
    }
}
