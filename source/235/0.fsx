// Define area of a rectangle
let area w h = w * h

// Define half of a value
let half x = x / 2

// Triangle area is half of it's rectangle
let triangleArea w h = half (area w h)

// Calculate triangle area with w=2; h=4
let myTriangle = triangleArea 2 4