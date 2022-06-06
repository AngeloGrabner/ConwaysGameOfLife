using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedWinConsole
{
    public class Utility
    {
        private int _width;
        public Utility(int width)
        {
            _width = width;
        }
        public int Convert2dTo1d(int x, int y)
        {
            return y * _width + x;
        }
        internal COORD Convert1dTo2d(short pos)
        {
            short y = 0, pos1 = pos;
            while (pos1 > _width)
            {
                pos1 -= (short)_width;
                y++;
            }
            return new COORD((short)(pos - (y * _width)), y);
        }
        internal short Convert1dToY(short pos)
        {
            short y = 0;
            while (pos > _width)
            {
                pos -= (short)_width;
                y++;
            }
            return y;
        }
    }
}
