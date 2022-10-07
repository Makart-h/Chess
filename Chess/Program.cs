using System;

namespace Chess;

public static class Program
{
    [STAThread]
    static void Main()
    {
        using Chess game = new();
        game.Run();
    }
}
