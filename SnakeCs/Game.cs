using System;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace SnakeCs
{
    class Game
    {
        private bool gameOver = false;
        private Snake snake;
        private Gfx.ConsoleGraphics graphics;
        private Direction lastDirection = Direction.Right;
        private Coord? foodLocation = null;
        private Coord gameSize = new Coord { X = 40, Y = 20 };
        private Coord gfxSize;

        // Amount of ms a single tick takes
        public int TickSpeed { get; set; }
        public int Score { get; private set; }

        public Game()
        {
            snake = new Snake(new Coord { X = 0, Y = 0 });
            RandomizeFood();
            gfxSize = new Coord { X = gameSize.X, Y = (short)(gameSize.Y + 3) };
            graphics = new Gfx.ConsoleGraphics(gfxSize.X, gfxSize.Y);

            graphics.SetTitle("Snake");
            graphics.SetAllowManualResize(false);
            graphics.SetCursorShown(false);
            graphics.WriteText("Use the arrow keys to move.", 0, 0);
            graphics.WriteText("Press any key to start the game", 0, 1);
            graphics.Flush();

            graphics.WaitForKey();
            graphics.Clear();

            Score = 0;
        }

        public void Tick()
        {
            graphics.Clear();
            ProcessInput();
            Update();
            Draw();

            // Sleep the thread for X milliseconds before advancing so the game doesn't process instantly
            System.Threading.Thread.Sleep(TickSpeed);
        }

        private void Draw()
        {
            foreach(var block in snake.Blocks)
            {
                graphics.DrawPoint(block.coords.X, block.coords.Y, Gfx.BackgroundColor.Red);
            }
            graphics.DrawPoint(foodLocation.Value.X, foodLocation.Value.Y, Gfx.BackgroundColor.Blue);
            graphics.DrawLine(0, gameSize.Y, (short)(gameSize.X - 1), gameSize.Y);
            graphics.DrawLine(0, gameSize.Y, 0, (short)(gfxSize.Y - 1));
            graphics.DrawLine((short)(gameSize.X - 1), gameSize.Y, (short)(gameSize.X - 1), (short)(gfxSize.Y - 1));
            graphics.DrawLine(0, (short)(gfxSize.Y - 1), (short)(gameSize.X - 1), (short)(gfxSize.Y - 1));
            graphics.WriteText($"Score: {Score}", 1, (short)(gameSize.Y + 1), 1);
            graphics.Flush();
        }

        private void Update()
        {
            // Picked up food
            if (foodLocation.Value == snake.Head.coords)
            {
                Score += 1;
                RandomizeFood();
                snake.Grow();
            }

            // Check for death
            if (snake.InsideSnake(snake.Head.coords, true))
            {
                gameOver = true;
            }
        }

        private void RandomizeFood()
        {
            foodLocation = new Coord
            {
                X = (short)RandomNumberGenerator.GetInt32(gameSize.X),
                Y = (short)RandomNumberGenerator.GetInt32(gameSize.Y)
            };
        }

        private void ProcessInput()
        {
            // If no input was given since the last tick, move in the same direction as previous tick
            if (!Console.KeyAvailable)
            {
                snake.Move(lastDirection, (short)(gameSize.X - 1), (short)(gameSize.Y - 1));
                return;
            }

            var lastKey = Console.ReadKey(true);
            // Clear input buffer to avoid keys building up over multiple frames
            while(Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
            Direction? direction = lastKey.Key switch
            {
                ConsoleKey.LeftArrow => Direction.Left,
                ConsoleKey.RightArrow => Direction.Right,
                ConsoleKey.UpArrow => Direction.Up,
                ConsoleKey.DownArrow => Direction.Down,
                _ => null
            };
            if (direction != null)
            {
                snake.Move(direction.Value, (short)(gameSize.X - 1), (short)(gameSize.Y - 1));
                lastDirection = direction.Value;
            } else
            {
                snake.Move(lastDirection, (short)(gameSize.X - 1), (short)(gameSize.Y - 1));
            }
        }

        public bool IsRunning()
        {
            return !gameOver;
        }
    }
}
