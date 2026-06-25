package p1

import (
	"fmt"
	"hackathon/utils"
)

// Solve takes the input lines and returns the answers for Part 1 and Part 2.
func Solve(lines []string) (string, string, error) {
	if len(lines) == 0 {
		return "", "", fmt.Errorf("no input lines provided")
	}

	part1 := solvePart1(lines)
	part2 := solvePart2(lines)

	return fmt.Sprintf("%v", part1), fmt.Sprintf("%v", part2), nil
}

func solvePart1(lines []string) int {
	total := 0
	for _, line := range lines {
		// Example: sum the length of each line
		total += len(line)
	}
	return total
}

func solvePart2(lines []string) int {
	// Example: sum numbers found on each line
	total := 0
	for _, line := range lines {
		numbers := utils.ParseInts(line)
		for _, n := range numbers {
			total += n
		}
	}
	return total
}
