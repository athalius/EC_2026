package utils

import (
	"bufio"
	"os"
	"strconv"
	"strings"
)

// ReadLines reads all lines from a text file.
func ReadLines(filePath string) ([]string, error) {
	file, err := os.Open(filePath)
	if err != nil {
		return nil, err
	}
	defer file.Close()

	var lines []string
	scanner := bufio.NewScanner(file)
	// Allocate 10MB buffer for extremely large input lines (standard scanner limit is 64KB)
	buf := make([]byte, 0, 1024*1024)
	scanner.Buffer(buf, 10*1024*1024)

	for scanner.Scan() {
		lines = append(lines, scanner.Text())
	}
	return lines, scanner.Err()
}

// ReadStdinLines reads all lines from standard input.
func ReadStdinLines() ([]string, error) {
	var lines []string
	scanner := bufio.NewScanner(os.Stdin)
	buf := make([]byte, 0, 1024*1024)
	scanner.Buffer(buf, 10*1024*1024)

	for scanner.Scan() {
		lines = append(lines, scanner.Text())
	}
	return lines, scanner.Err()
}

// Atoi converts a string to an integer. Panics or returns 0 on error,
// which is useful for hackathons to skip error checking boilerplate.
func Atoi(s string) int {
	val, err := strconv.Atoi(strings.TrimSpace(s))
	if err != nil {
		return 0
	}
	return val
}

// Atoi64 converts a string to an int64.
func Atoi64(s string) int64 {
	val, err := strconv.ParseInt(strings.TrimSpace(s), 10, 64)
	if err != nil {
		return 0
	}
	return val
}

// ParseInts splits a string by whitespace or commas and returns integers.
func ParseInts(s string) []int {
	// Standardize commas to spaces for easy splitting
	normalized := strings.ReplaceAll(s, ",", " ")
	fields := strings.Fields(normalized)
	result := make([]int, len(fields))
	for i, f := range fields {
		result[i] = Atoi(f)
	}
	return result
}

// ParseRuneGrid parses a slice of string lines into a 2D rune array.
func ParseRuneGrid(lines []string) [][]rune {
	grid := make([][]rune, len(lines))
	for i, line := range lines {
		grid[i] = []rune(line)
	}
	return grid
}

// ParseIntGrid parses a grid of digits (like a maze with height values 0-9) into a 2D int array.
func ParseIntGrid(lines []string) [][]int {
	grid := make([][]int, len(lines))
	for i, line := range lines {
		row := make([]int, len(line))
		for j, r := range line {
			row[j] = int(r - '0')
		}
		grid[i] = row
	}
	return grid
}
