package utils

import (
	"fmt"
)

// Abs returns the absolute value of an integer.
func Abs[T int | int64 | int32 | int16 | int8](x T) T {
	if x < 0 {
		return -x
	}
	return x
}

// Min returns the minimum of two integers.
func Min[T int | int64 | int32 | int16 | int8](a, b T) T {
	if a < b {
		return a
	}
	return b
}

// Max returns the maximum of two integers.
func Max[T int | int64 | int32 | int16 | int8](a, b T) T {
	if a > b {
		return a
	}
	return b
}

// GCD returns the Greatest Common Divisor of two integers.
func GCD[T int | int64 | int32 | int16 | int8](a, b T) T {
	for b != 0 {
		t := b
		b = a % b
		a = t
	}
	return Abs(a)
}

// LCM returns the Least Common Multiple of two integers.
func LCM[T int | int64 | int32 | int16 | int8](a, b T) T {
	if a == 0 || b == 0 {
		return 0
	}
	return Abs(a * b) / GCD(a, b)
}

// Point represents a 2D integer coordinate.
type Point struct {
	X, Y int
}

// NewPoint creates a new Point.
func NewPoint(x, y int) Point {
	return Point{X: x, Y: y}
}

// Add returns the vector sum of two points.
func (p Point) Add(other Point) Point {
	return Point{X: p.X + other.X, Y: p.Y + other.Y}
}

// Sub returns the vector difference of two points.
func (p Point) Sub(other Point) Point {
	return Point{X: p.X - other.X, Y: p.Y - other.Y}
}

// Mul returns the scalar product of the point.
func (p Point) Mul(scalar int) Point {
	return Point{X: p.X * scalar, Y: p.Y * scalar}
}

// Manhattan returns the Manhattan distance between two points.
func (p Point) Manhattan(other Point) int {
	return Abs(p.X-other.X) + Abs(p.Y-other.Y)
}

// Neighbors4 returns the 4 cardinal neighbors (Up, Down, Left, Right).
func (p Point) Neighbors4() []Point {
	return []Point{
		{X: p.X, Y: p.Y - 1}, // Up
		{X: p.X, Y: p.Y + 1}, // Down
		{X: p.X - 1, Y: p.Y}, // Left
		{X: p.X + 1, Y: p.Y}, // Right
	}
}

// Neighbors8 returns all 8 neighbors (including diagonals).
func (p Point) Neighbors8() []Point {
	return []Point{
		{X: p.X, Y: p.Y - 1}, // Up
		{X: p.X, Y: p.Y + 1}, // Down
		{X: p.X - 1, Y: p.Y}, // Left
		{X: p.X + 1, Y: p.Y}, // Right
		{X: p.X - 1, Y: p.Y - 1}, // Up-Left
		{X: p.X + 1, Y: p.Y - 1}, // Up-Right
		{X: p.X - 1, Y: p.Y + 1}, // Down-Left
		{X: p.X + 1, Y: p.Y + 1}, // Down-Right
	}
}

// String returns a standard string representation of the Point.
func (p Point) String() string {
	return fmt.Sprintf("(%d,%d)", p.X, p.Y)
}

// Common Directions in standard grid layouts
var (
	DirUp    = Point{X: 0, Y: -1}
	DirDown  = Point{X: 0, Y: 1}
	DirLeft  = Point{X: -1, Y: 0}
	DirRight = Point{X: 1, Y: 0}

	// Cardinal map for U, D, L, R
	CharToDir = map[rune]Point{
		'U': DirUp, 'u': DirUp, '^': DirUp,
		'D': DirDown, 'd': DirDown, 'v': DirDown,
		'L': DirLeft, 'l': DirLeft, '<': DirLeft,
		'R': DirRight, 'r': DirRight, '>': DirRight,
	}
)
