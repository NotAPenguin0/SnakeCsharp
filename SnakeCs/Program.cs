using System;

namespace SnakeCs
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.TickSpeed = 200; // in milliseconds
            while (game.IsRunning())
            {
                game.Tick();
            }
            Console.Clear();
            Console.WriteLine("Game Over! Thanks for playing!");
        }
    }
}
