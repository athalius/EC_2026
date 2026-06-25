package main

import (
	"flag"
	"fmt"
	"log"
	"os"
	"path/filepath"
	"time"

	"hackathon/problems/p1"
	"hackathon/utils"
)

type SolverFunc func([]string) (string, string, error)

// Register solvers here
var solvers = map[int]SolverFunc{
	1: p1.Solve,
}

func main() {
	problemFlag := flag.Int("p", 1, "Problem number to run (e.g. -p 1)")
	inputFlag := flag.String("i", "", "Path to input file (e.g. -i problems/p1/input.txt). If empty, defaults to problems/p[N]/input.txt")
	stdinFlag := flag.Bool("stdin", false, "Read input from stdin instead of a file")
	pdfFlag := flag.String("pdf", "", "Path to PDF file to extract text and print (useful for viewing prompt)")

	flag.Parse()

	// If pdf flag is provided, extract text and print it
	if *pdfFlag != "" {
		fmt.Printf("--- Extracting Text from PDF: %s ---\n", *pdfFlag)
		text, err := utils.ExtractPDFText(*pdfFlag)
		if err != nil {
			log.Fatalf("Error reading PDF: %v", err)
		}
		fmt.Println(text)
		return
	}

	solver, exists := solvers[*problemFlag]
	if !exists {
		log.Fatalf("Problem %d solver is not implemented or registered.", *problemFlag)
	}

	var lines []string
	var err error

	if *stdinFlag {
		fmt.Println("Reading input from stdin (press Ctrl+D/Ctrl+Z to finish)...")
		lines, err = utils.ReadStdinLines()
		if err != nil {
			log.Fatalf("Failed to read from stdin: %v", err)
		}
	} else {
		inputFile := *inputFlag
		if inputFile == "" {
			inputFile = filepath.Join("problems", fmt.Sprintf("p%d", *problemFlag), "input.txt")
		}

		if _, err := os.Stat(inputFile); os.IsNotExist(err) {
			log.Fatalf("Input file does not exist: %s. Use -i to specify a file or -stdin to read from stdin.", inputFile)
		}

		lines, err = utils.ReadLines(inputFile)
		if err != nil {
			log.Fatalf("Failed to read input file %s: %v", inputFile, err)
		}
	}

	fmt.Printf("=== Running Problem %d (%d lines of input) ===\n", *problemFlag, len(lines))

	startTime := time.Now()
	part1, part2, err := solver(lines)
	duration := time.Since(startTime)

	if err != nil {
		log.Fatalf("Solver returned error: %v", err)
	}

	fmt.Println()
	fmt.Printf("Part 1: %s\n", part1)
	fmt.Printf("Part 2: %s\n", part2)
	fmt.Println()
	fmt.Printf("Execution time: %s\n", duration)
	fmt.Println("=====================================")
}
