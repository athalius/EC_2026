package utils

import (
	"container/heap"
)

// Set is a generic set structure.
type Set[T comparable] map[T]struct{}

// NewSet creates a new empty set.
func NewSet[T comparable]() Set[T] {
	return make(Set[T])
}

// Add inserts an element into the set.
func (s Set[T]) Add(val T) {
	s[val] = struct{}{}
}

// Remove deletes an element from the set.
func (s Set[T]) Remove(val T) {
	delete(s, val)
}

// Contains checks if the set contains an element.
func (s Set[T]) Contains(val T) bool {
	_, ok := s[val]
	return ok
}

// Size returns the number of elements in the set.
func (s Set[T]) Size() int {
	return len(s)
}

// Keys returns all elements in the set.
func (s Set[T]) Keys() []T {
	keys := make([]T, 0, len(s))
	for k := range s {
		keys = append(keys, k)
	}
	return keys
}

// Queue is a generic slice-based FIFO queue.
type Queue[T any] []T

// NewQueue creates a new empty queue.
func NewQueue[T any]() *Queue[T] {
	return &Queue[T]{}
}

// Push adds an element to the end of the queue.
func (q *Queue[T]) Push(val T) {
	*q = append(*q, val)
}

// Pop removes and returns the first element from the queue.
func (q *Queue[T]) Pop() (T, bool) {
	if len(*q) == 0 {
		var zero T
		return zero, false
	}
	val := (*q)[0]
	*q = (*q)[1:]
	return val, true
}

// IsEmpty returns true if the queue has no elements.
func (q *Queue[T]) IsEmpty() bool {
	return len(*q) == 0
}

// Size returns the number of elements in the queue.
func (q *Queue[T]) Size() int {
	return len(*q)
}

// Stack is a generic slice-based LIFO stack.
type Stack[T any] []T

// NewStack creates a new empty stack.
func NewStack[T any]() *Stack[T] {
	return &Stack[T]{}
}

// Push adds an element to the top of the stack.
func (s *Stack[T]) Push(val T) {
	*s = append(*s, val)
}

// Pop removes and returns the top element of the stack.
func (s *Stack[T]) Pop() (T, bool) {
	if len(*s) == 0 {
		var zero T
		return zero, false
	}
	idx := len(*s) - 1
	val := (*s)[idx]
	*s = (*s)[:idx]
	return val, true
}

// IsEmpty returns true if the stack has no elements.
func (s *Stack[T]) IsEmpty() bool {
	return len(*s) == 0
}

// Size returns the number of elements in the stack.
func (s *Stack[T]) Size() int {
	return len(*s)
}

// PQItem represents an item in the priority queue.
type PQItem[T any] struct {
	Value    T
	Priority int
	index    int
}

// pqImpl is the internal implementation of heap.Interface.
type pqImpl[T any] []*PQItem[T]

func (pq pqImpl[T]) Len() int { return len(pq) }

// Less is defined here for a min-heap (lower value = higher priority).
// Change to > for a max-heap.
func (pq pqImpl[T]) Less(i, j int) bool {
	return pq[i].Priority < pq[j].Priority
}

func (pq pqImpl[T]) Swap(i, j int) {
	pq[i], pq[j] = pq[j], pq[i]
	pq[i].index = i
	pq[j].index = j
}

func (pq *pqImpl[T]) Push(x any) {
	n := len(*pq)
	item := x.(*PQItem[T])
	item.index = n
	*pq = append(*pq, item)
}

func (pq *pqImpl[T]) Pop() any {
	old := *pq
	n := len(old)
	item := old[n-1]
	old[n-1] = nil  // avoid memory leak
	item.index = -1 // for safety
	*pq = old[0 : n-1]
	return item
}

// PriorityQueue is a generic wrapper around the container/heap implementation.
// By default, it is a Min-Priority Queue (lowest priority number comes out first).
type PriorityQueue[T any] struct {
	impl pqImpl[T]
}

// NewPriorityQueue creates an empty Priority Queue.
func NewPriorityQueue[T any]() *PriorityQueue[T] {
	pq := &PriorityQueue[T]{impl: make(pqImpl[T], 0)}
	heap.Init(&pq.impl)
	return pq
}

// Push adds an item to the priority queue.
func (pq *PriorityQueue[T]) Push(val T, priority int) {
	item := &PQItem[T]{
		Value:    val,
		Priority: priority,
	}
	heap.Push(&pq.impl, item)
}

// Pop removes and returns the item with the minimum priority number.
func (pq *PriorityQueue[T]) Pop() (T, int, bool) {
	if len(pq.impl) == 0 {
		var zero T
		return zero, 0, false
	}
	item := heap.Pop(&pq.impl).(*PQItem[T])
	return item.Value, item.Priority, true
}

// IsEmpty checks if the priority queue is empty.
func (pq *PriorityQueue[T]) IsEmpty() bool {
	return len(pq.impl) == 0
}

// Size returns the size of the priority queue.
func (pq *PriorityQueue[T]) Size() int {
	return len(pq.impl)
}
