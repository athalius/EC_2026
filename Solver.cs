using System;
using Hackathon.Utils;

namespace Hackathon
{
    public static class Solver
    {
        public static (string Part1, string Part2) Solve(string[] lines)
        {
            if (lines.Length == 0)
            {
                throw new ArgumentException("No lines of input provided.");
            }

            var part1 = SolvePart1(lines);
            var part2 = SolvePart2(lines);

            return (part1.ToString(), part2.ToString());
        }

        private static int SolvePart1(string[] lines)
        {
            int total = 0;
            foreach (var line in lines)
            {
                total += line.Length;
            }
            return total;
        }

        private static int SolvePart2(string[] lines)
        {
            int total = 0;
            foreach (var line in lines)
            {
                var numbers = Io.ParseInts(line);
                foreach (var n in numbers)
                {
                    total += n;
                }
            }
            return total;
        }
    }
}
