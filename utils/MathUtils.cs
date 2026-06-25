using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hackathon.Utils
{
    public static class MathUtils
    {
        // Abs, Min, and Max for any numeric type are built-in in modern .NET via Math.Abs, Math.Min, Math.Max,
        // but GCD and LCM are handy additions.

        public static T GCD<T>(T a, T b) where T : INumber<T>
        {
            a = T.Abs(a);
            b = T.Abs(b);
            while (b != T.Zero)
            {
                T temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static T LCM<T>(T a, T b) where T : INumber<T>
        {
            if (a == T.Zero || b == T.Zero) return T.Zero;
            return T.Abs(a * b) / GCD(a, b);
        }
    }

    public record struct Point(int X, int Y)
    {
        public static Point Up => new(0, -1);
        public static Point Down => new(0, 1);
        public static Point Left => new(-1, 0);
        public static Point Right => new(1, 0);

        public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
        public static Point operator *(Point p, int scalar) => new(p.X * scalar, p.Y * scalar);

        public int ManhattanDistance(Point other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

        public Point[] Neighbors4() => new Point[]
        {
            new(X, Y - 1), // Up
            new(X, Y + 1), // Down
            new(X - 1, Y), // Left
            new(X + 1, Y)  // Right
        };

        public Point[] Neighbors8() => new Point[]
        {
            new(X, Y - 1), // Up
            new(X, Y + 1), // Down
            new(X - 1, Y), // Left
            new(X + 1, Y), // Right
            new(X - 1, Y - 1), // Up-Left
            new(X + 1, Y - 1), // Up-Right
            new(X - 1, Y + 1), // Down-Left
            new(X + 1, Y + 1)  // Down-Right
        };

        public override string ToString() => $"({X},{Y})";

        public static Dictionary<char, Point> CharToDir = new()
        {
            { 'U', Up }, { 'u', Up }, { '^', Up },
            { 'D', Down }, { 'd', Down }, { 'v', Down },
            { 'L', Left }, { 'l', Left }, { '<', Left },
            { 'R', Right }, { 'r', Right }, { '>', Right }
        };
    }
}
