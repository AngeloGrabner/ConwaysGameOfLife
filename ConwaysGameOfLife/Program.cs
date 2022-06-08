class Program
{
    public static void Main()
    {
        Game game = new Game(150,60, 500);
        game.RunEditor();
        //game.LoadFile(@"C:\Users\AG\Desktop\Schule\474843811.txt");
        game.RunGame();
        //ExtendedWinConsole.ExConsole.Write(game.SafeFlie(@"C:\Users\AG\Desktop\Schule"));
    }
}
