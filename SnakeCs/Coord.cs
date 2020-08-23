using System;

namespace SnakeCs
{
    public struct Coord : IEquatable<Coord>
    {
        public short X { get; set; }
        public short Y { get; set; }

        public bool Equals(Coord other)
        {
            return X == other.X && Y == other.Y;
        }

        public static bool operator==(Coord a, Coord b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(Coord a, Coord b)
        {
            return !a.Equals(b);
        }
    }
}
