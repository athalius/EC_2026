using System;
using System.Diagnostics;
using System.IO;
using Hackathon.Utils;

namespace Hackathon
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string inputFile = "input.txt";
            bool readFromStdin = false;
            string? pdfFile = null;

            // Command line arguments parsing
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-i" && i + 1 < args.Length)
                {
                    inputFile = args[++i];
                }
                else if (args[i] == "-stdin")
                {
                    readFromStdin = true;
                }
                else if (args[i] == "-pdf" && i + 1 < args.Length)
                {
                    pdfFile = args[++i];
                }
            }

            if (pdfFile != null)
            {
                Console.WriteLine($"--- Extracting Text from PDF: {pdfFile} ---");
                try
                {
                    string text = PdfUtils.ExtractPDFText(pdfFile);
                    Console.WriteLine(text);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error reading PDF: {ex.Message}");
                }
                return;
            }

            string[] lines;
            try
            {
                if (readFromStdin)
                {
                    Console.WriteLine("Reading input from stdin (press Ctrl+Z then Enter to finish on Windows, or Ctrl+D on Unix)...");
                    lines = Io.ReadStdinLines();
                }
                else
                {
                    if (!File.Exists(inputFile))
                    {
                        Console.Error.WriteLine($"Input file does not exist: {inputFile}. Use -i to specify a file path or -stdin to read from stdin.");
                        return;
                    }

                    lines = Io.ReadLines(inputFile);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to read input: {ex.Message}");
                return;
            }

            Console.WriteLine($"=== Running Solver ({lines.Length} lines of input) ===");

            var sw = Stopwatch.StartNew();
            try
            {
                var (part1, part2) = Solver.Solve(lines);
                sw.Stop();

                Console.WriteLine();
                Console.WriteLine($"Part 1: {part1}");
                Console.WriteLine($"Part 2: {part2}");
                Console.WriteLine();

                double ms = (double)sw.ElapsedTicks / TimeSpan.TicksPerMillisecond;
                if (ms < 1.0)
                {
                    double us = (double)sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000.0);
                    Console.WriteLine($"Execution time: {us:F2} μs");
                }
                else
                {
                    Console.WriteLine($"Execution time: {ms:F2} ms");
                }
                Console.WriteLine("=====================================");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.Error.WriteLine($"Solver error: {ex}");
            }
        }
    }
}
