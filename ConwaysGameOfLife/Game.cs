using ExtendedWinConsole;
class Game // to be added: reading and writing gamestate to files
{
    bool _isEditing = true, _isRunning = true;
    char _inputChar;
    short _width, _height;
    CHAR_INFO[] _dGameBoard;
    bool[] _lGameBoard;
    int _playerX, _playerY, _delay;
    Utility _utility;
    public Game(short width = 100, short height = 50, int delay = 100)
    {
        if (width < ExConsole.MinWidth || height < ExConsole.MinHeight ||delay < 0)
        {
            throw new ArgumentException("width or height or delay");
        }
        _delay = delay;
        _width = width;
        _height = height;
        _utility = new(_width);
        ExConsole.SetMaximumBufferSize((short)(_width), (short)(_height));
        ExConsole.SetBufferSize((short)(_width), (short)(_height));
        ExConsole.SetWindowSize(1300,1300); 

        ExConsole.SetFont(8, 8);
        ExConsole.SetCursorVisiblity(false);
        _dGameBoard = ExConsole.OutputBuffer;
        _lGameBoard = new bool[_dGameBoard.Length];
        for (int i = 0; i < _lGameBoard.Length; i++)
        {
            _lGameBoard[i] = false;
        }
        _playerX = width / 2;
        _playerY = height / 2;
    }
    public void Run()
    {
        DisplayGameInfo();
        while (true)
        {
            _inputChar = GetInput();
            if (_inputChar == 'e')
            {
                break;
            }
            else if (_inputChar == 'q')
            {
                ChangeLiefeStatus();
            }
            else
            {
                Move(_inputChar);
            }
            DrawBoard();
        }
        RunGame();
    }
    char GetInput()
    { 
        return ExConsole.ReadKey().KeyChar;
    }
    void Move(char direction)
    {
        int x = 0, y = 0;
        switch (direction)
        {
            case 'W':
            case 'w':
                y--;
                break;
            case 'S':
            case 's':
                y++;
                break;
            case 'A':
            case 'a':
                x--;
                break;
            case 'D':
            case 'd':
                x++;
                break;
            default:
                return;
        }
        if (_playerX + x < _width - 1 && _playerX + x > 0 && _playerY + y < _height - 1 && _playerY + y > 0)
        {
            _playerX += x;
            _playerY += y;
        }
    }
    void DisplayGameInfo()
    {
        ExConsole.WriteLine("controls:\nuse 'W A S D' to move\nuse 'Q' to change the life status of a cell\nuse 'E' to start the game\npress any key to start");
        ExConsole.ReadKey();
        ExConsole.Clear(false);
    }
    void ChangeLiefeStatus()
    {
        bool temp = _lGameBoard[_utility.Convert2dTo1d(_playerX, _playerY)];
        temp = !temp;
        _lGameBoard[_utility.Convert2dTo1d(_playerX, _playerY)] = temp;
    }
    void DrawBorder()
    {
        char[] _borderTiles = new char[6] { '╔', '╗', '╚', '╝', '║', '═' };
        for (int i = 1; i < _width - 1; i++) // top line
        {
            _dGameBoard[i].UnicodeChar = _borderTiles[(int)BorderType.HoriziontalLine];
        }
        for (int i = _width * _height - _width; i < _dGameBoard.Length; i++) // bottom line
        {
            _dGameBoard[i].UnicodeChar = _borderTiles[(int)BorderType.HoriziontalLine];
        }
        for (int i = _width; i < _width * _height - 1; i += _width) // left line
        {
            _dGameBoard[i].UnicodeChar = _borderTiles[(int)BorderType.VerticalLine];
        }
        for (int i = _width * 2 - 1; i < _dGameBoard.Length; i += _width) // right line
        {
            _dGameBoard[i].UnicodeChar = _borderTiles[(int)BorderType.VerticalLine];
        }
        // corners
        _dGameBoard[0].UnicodeChar = _borderTiles[(int)BorderType.TopLeft];

        _dGameBoard[_width - 1].UnicodeChar = _borderTiles[(int)BorderType.TopRight];

        _dGameBoard[_utility.Convert2dTo1d(0, _height - 1)].UnicodeChar = _borderTiles[(int)BorderType.BottomLeft]; // -1 could be wrong

        _dGameBoard[(_height * _width) - 1].UnicodeChar = _borderTiles[(int)BorderType.BottomRight];
    }
    void DrawBoard()
    {
        for (int i = 0; i < _lGameBoard.Length; i++)
        {
            if (_lGameBoard[i])
            {
                _dGameBoard[i].UnicodeChar = '\u2588';
            }
        }
        if (_isEditing)
        {
            int playerPos = _utility.Convert2dTo1d(_playerX, _playerY);
            _dGameBoard[playerPos].UnicodeChar = 'X';
            if (_lGameBoard[playerPos])
            {
                _dGameBoard[playerPos].Attributes = (ushort)ConsoleColor.Green;
            }
            else
            {
                _dGameBoard[playerPos].Attributes = (ushort)ConsoleColor.Red;
            }
        }
        DrawBorder();
        ExConsole.UpdateBuffer();
    }
    void RunGame()
    {
        _isEditing = false;
        Thread thd = new Thread(new ThreadStart(GetInputAsync));
        thd.Start();
        _inputChar = ' ';
        while (_inputChar != 'e')
        {
            UpdateBoard();
            DrawBoard();
            Thread.Sleep(_delay);
        }
        _isRunning = false; 
        thd.Join();
    }
    void GetInputAsync()
    {
        while (_isRunning)
        {
            _inputChar = ExConsole.ReadKey().KeyChar;
            Thread.Sleep(100);
        }
    }
    void UpdateBoard()
    {
        bool[] temp = new bool[_lGameBoard.Length];
        for (int i = 0; i < _lGameBoard.Length; i++)
        {
            temp[i] = false;
            int count = GetSumOfNeighborAlive(i);
            if (count == -1)
            {
                continue;
            }
            if (count == 3 && !_lGameBoard[i]) // Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
            {
                temp[i] = true;
            }
            else if (count < 2 && _lGameBoard[i]) // Any live cell with fewer than two live neighbours dies, as if by underpopulation.
            {
                temp[i] = false;
            }
            else if ((count == 2 || count == 3) && _lGameBoard[i]) // Any live cell with two or three live neighbours lives on to the next generation.
            {
                temp[i] = true;
            }
            else if (count > 3 && _lGameBoard[i]) // Any live cell with more than three live neighbours dies, as if by overpopulation.
            {
                temp[i] = false;
            }
        }
        for (int i = 0; i < temp.Length; i++)
        {
            _lGameBoard[i] = temp[i];
        }
    }
    int GetSumOfNeighborAlive(int index)
    {
        COORD center = _utility.Convert1dTo2d((short)index);
        if (center.x < 1 || center.x > (_width-2) || center.y < 1|| center.y > (_height-2))
        {
            return -1;
        }
        int aliveCount = 0;
        for (int i = -1; i < 2;i++)
        {
            for (int j = -1; j < 2;j++)
            {
                if (i == 0 && 0 == j) 
                {
                    continue;
                }
                if (_lGameBoard[_utility.Convert2dTo1d(center.x+i, center.y+j)])
                {
                    aliveCount++;
                }
            }
        }
        return aliveCount;
    }
}