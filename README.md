# Sudoku Challenge

## Overview
Sudoku is a number placement puzzle based on a 9x9 grid with several given numbers.  The object is to place the numbers 1 to 9 in the empty squares so that each row, each column, and each 3x3 box contains the numbers 1-9 only once. 

The objective of this exercise is to develop a program that solves Sudoku puzzles by filling in the empty blanks without violating any of the constraints defined in the rules, above.

## Example Sudoku Matrix
![alt text](https://i5.walmartimages.com/asr/7af38b61-6453-4ab3-88b8-2ca4568b9d77.623c1f8ed6e4bdc3799ac3c2c0cedea2.jpeg?odnHeight=612&odnWidth=612&odnBg=FFFFFF)

## Example Input Data
![alt text](https://i5.walmartimages.com/asr/7af38b61-6453-4ab3-88b8-2ca4568b9d77.623c1f8ed6e4bdc3799ac3c2c0cedea2.jpeg?odnHeight=612&odnWidth=612&odnBg=FFFFFF)
![alt text](https://i5.walmartimages.com/asr/7af38b61-6453-4ab3-88b8-2ca4568b9d77.623c1f8ed6e4bdc3799ac3c2c0cedea2.jpeg?odnHeight=612&odnWidth=612&odnBg=FFFFFF)
![alt text](https://i5.walmartimages.com/asr/7af38b61-6453-4ab3-88b8-2ca4568b9d77.623c1f8ed6e4bdc3799ac3c2c0cedea2.jpeg?odnHeight=612&odnWidth=612&odnBg=FFFFFF)
![alt text](https://i5.walmartimages.com/asr/7af38b61-6453-4ab3-88b8-2ca4568b9d77.623c1f8ed6e4bdc3799ac3c2c0cedea2.jpeg?odnHeight=612&odnWidth=612&odnBg=FFFFFF)

## Solution
Solution
My solution works by traversing the diagonal of the Sudoku matrix and creating solution sets. 
 Solution sets are basically horizontal and vertical pairs that intersect at the diagonal.  So think of an { X, Y } coordinate and grabbing the corresponding row and columns that have intersection at { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 5 }, { 6, 6 }, { 7, 7 }, { 8, 8 }.
 
We then solve each set, we do this by taking calculating the delta of allowed range {1-9}, and the numbers provided for each row and column.   

Once we have the delta values, we then calculate all possible permutations for the delta values.  We reduce the computational power needed to solve the puzzle, by not having to calculate every unnecessary possibility.  This is further optimized by utilizing a hash table to store permutation data in memory to increase the speed of generation of subsequent permutations using the same delta data.

When we have all the sets and permutation data generated, we than attempt to solve the entire Sudoku.  The sets we generated, contain all possible solutions for given row, column.  

We loop through our sets again only this time we generate solution matrix, each solution is a clone of the game data matrix we loaded in, and we add it to a List.  It is possible to have more than one solution matrix, and this solution will provide every combination where applicable.

The solution matrices are then filtered to only allow sets that meet the intersection restraints.  Since all the sets where already precalculated all rows, columns and diagonal set will total 45.

1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9 = 45

## Methods

## Features
-    lorem ipsum

# Getting Started 
 1. lorem ipsum
 2. lorem ipsum
    - `npm install`
 
