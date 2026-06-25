using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hackathon.Utils
{
    public static class Io
    {
        // Read lines from a file with high buffer size
        public static string[] ReadLines(string path)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(path, System.Text.Encoding.UTF8, true, 1024 * 1024))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines.ToArray();
        }

        // Read lines from stdin
        public static string[] ReadStdinLines()
        {
            var lines = new List<string>();
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                lines.Add(line);
            }
            return lines.ToArray();
        }

        // Safe conversion of string to int, returning 0 on failure
        public static int Atoi(string s)
        {
            return int.TryParse(s.Trim(), out var val) ? val : 0;
        }

        // Safe conversion of string to long, returning 0 on failure
        public static long Atoi64(string s)
        {
            return long.TryParse(s.Trim(), out var val) ? val : 0;
        }

        // Parse list of integers separated by spaces or commas
        public static int[] ParseInts(string s)
        {
            var normalized = s.Replace(",", " ");
            return normalized.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(Atoi)
                             .ToArray();
        }

        // Convert string lines to a char[][] grid
        public static char[][] ParseCharGrid(string[] lines)
        {
            return lines.Select(l => l.ToCharArray()).ToArray();
        }

        // Convert digit lines (e.g. "0123") into an int[][] grid
        public static int[][] ParseIntGrid(string[] lines)
        {
            return lines.Select(l => l.Select(c => c - '0').ToArray()).ToArray();
        }
    }
}
