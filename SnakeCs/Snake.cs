using System;
using System.Collections.Generic;

namespace SnakeCs
{
    public class Snake
    {
        private bool growNextMove = false;

        public class Block
        {
            public bool isHead = false;
            public Coord coords;
        }

        public List<Block> Blocks { get; }
        public Block Head => Blocks[0];

        public Snake(Coord startPos)
        {
            Blocks = new List<Block>();
            Blocks.Add(new Block { isHead = true, coords = startPos });
        }

        public bool InsideSnake(Coord pos, bool excludeHead = false)
        {
            foreach (Block block in Blocks)
            {
                if (excludeHead && block.isHead) continue;
                if (block.coords == pos) { return true; }
            }
            return false;
        }

        public void Move(Direction direction, short maxX, short maxY)
        {
            // Add a new block in the specified direction, this is the new head
            Blocks[0].isHead = false;
            Coord oldFrontPos = Blocks[0].coords;
            Coord newFrontPos = direction switch
            {
                Direction.Left => new Coord { X = (short)(oldFrontPos.X - 1), Y = oldFrontPos.Y },
                Direction.Right => new Coord { X = (short)(oldFrontPos.X + 1), Y = oldFrontPos.Y },
                Direction.Up => new Coord { X = oldFrontPos.X, Y = (short)(oldFrontPos.Y - 1) },
                Direction.Down => new Coord { X = oldFrontPos.X, Y = (short)(oldFrontPos.Y + 1) },
                _ => throw new NotImplementedException("Invalid direction")
            };

            // Implement wrapping
            if (newFrontPos.X < 0) newFrontPos.X = maxX;
            if (newFrontPos.X > maxX) newFrontPos.X = 0;
            if (newFrontPos.Y < 0) newFrontPos.Y = maxY;
            if (newFrontPos.Y > maxY) newFrontPos.Y = 0;

            Blocks.Insert(0, new Block { isHead = true, coords = newFrontPos});
            // We implement growing by not removing the tail block.
            if (!growNextMove)
            {
                // Remove last block
                Blocks.RemoveAt(Blocks.Count - 1);
            }
            growNextMove = false;
        }

        public void Grow()
        {
            growNextMove = true;
        }
    }
}
