# Sudoku Challenge

## Overview
Sudoku is a number placement puzzle based on a 9x9 grid with several given numbers.  The object is to place the numbers 1 to 9 in the empty squares so that each row, each column, and each 3x3 box contains the numbers 1-9 only once. 

The objective of this exercise is to develop a program that solves Sudoku puzzles by filling in the empty blanks without violating any of the constraints defined in the rules, above.

## Example Sudoku Matrix
![alt text](https://github.com/kevingarabedian2/sudoku-challenge/blob/main/Example_Matrix.png?raw=true)

## Example Input Data
![alt text](https://github.com/kevingarabedian2/sudoku-challenge/blob/main/Puzzle1.png?raw=true) 
![alt text](https://github.com/kevingarabedian2/sudoku-challenge/blob/main/puzzle2.png?raw=true) 
![alt text](https://github.com/kevingarabedian2/sudoku-challenge/blob/main/Puzzle3.png?raw=true) 
![alt text](https://github.com/kevingarabedian2/sudoku-challenge/blob/main/Puzzle4.png?raw=true) 
![alt text](https://github.com/kevingarabedian2/sudoku-challenge/blob/main/Puzzle5.png?raw=true) 

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
```csharp
        private bool Load(string file)
        {
            // Perform Integrity Check (validate input)
            // read from data/input/puzzle{0}.txt 

            is_loaded = true;
            string[] data = System.IO.File.ReadAllLines(file); // read all lines from data file into string array

            for (int y = 0; y < data.Length; y++) // loop through all lines/rows from read data
            {
                string temp = data[y].Trim(); // trim data to normalize and remove white spaces, if exist
                if (temp.Length == 0) // validate line/row to make sure the value is not null/empty
                {
                    continue; // if null/empty skip
                }

                if (temp.Length == 9) // make sure data is correct length, input should be exactly 9
                {
                    for (int x = 8; x > -1; x--) // loop through char values in string
                    {
                        switch (temp[x]) // check char against allowed values, ignore non-allowed
                        {
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                matrix.Add(new Point(x, y), (int)Char.GetNumericValue(temp[x])); // for values 1-9, assign to matrix game data, chast char value to int and 
                                break;
                            case 'X':
                                matrix.Add(new Point(x, y), 0); // if value is 'X', store game value as 0 to make calculation easier, and add it to matrix
                                break;
                        }
                    }
                }
            }
            return true; // data loaded successfully
        }
```

```csharp
        private SodokuSet GenerateSet(int x, int y)
        {
            SodokuSet sodokuSet = new SodokuSet();
            SetData setData = new SetData();
            setData.Intersection = new Point(x, y);
            setData.Type = SetType.Vertical;

            for (int y2 = 0; y2 < 9; y2++)
            {
                if (!setData.Plots.ContainsKey(new Point(y, y2)))
                {
                    setData.Plots.Add(new Point(x, y2), matrix[new Point(x, y2)]);
                }
            }
            sodokuSet.Plots.Add(setData);

            setData = new SetData();
            setData.Type = SetType.Horizontal; // 
            for (int x2 = 0; x2 < 9; x2++)
            {
                if (!setData.Plots.ContainsKey(new Point(x2, y)))
                {
                    setData.Plots.Add(new Point(x2, y), matrix[new Point(x2, y)]);
                }
            }
            sodokuSet.Plots.Add(setData);

            if (x == y)
            {
                setData = new SetData();
                setData.Type = SetType.Diagnal;
                for (int i = 0; i < 9; i++)
                {
                    if (!setData.Plots.ContainsKey(new Point(i, i)))
                    {
                        setData.Plots.Add(new Point(i, i), matrix[new Point(i, i)]);
                    }
                }
                sodokuSet.Plots.Add(setData);

                setData = new SetData();
                setData.Type = SetType.Diagnal;
                for (int i = 8; i > -1; i--)
                {
                    if (!setData.Plots.ContainsKey(new Point(i, i)))
                    {
                        setData.Plots.Add(new Point(i, (8 - i)), matrix[new Point(i, 8 - i)]);
                    }
                }
                sodokuSet.Plots.Add(setData);
            }
            return sodokuSet;
        }
```
```csharp
        public void Solve()
        {
            for (int x = 0; x < 9; x++) // Traverse diag to obtain all intersecting sets
            {
                solution_sets.Add(x, GenerateSet(x, x)); // Create sets of horizontal and vertical pairs
            }

            foreach (var x in solution_sets.Keys) // loop through the calculated pairs and generate solution combos
            {
                var set = solution_sets[x];
                List<int> tmpPlots = new List<int>();

                Console.WriteLine("--- Processing Sets ---");
                foreach (var obj in set.Plots)
                {
                    string output = "";

                    foreach (var xy in obj.Plots.Keys)
                    {
                        int val = obj.Plots[xy];
                        tmpPlots.Add(val);
                        output += val + " ";
                    }

                    if (obj.Type == SetType.Diagnal)  // Ignore diagnal pairs, not needed
                    {
                        continue;
                    }

                    output += " => " + obj.Type.ToString();

                    List<int> delta = allowed_values.Except(tmpPlots).ToList(); // calculate delts for missing possible values for data set
                    obj.Delta = delta.ToList();

                    string delta_values = "";
                    foreach (var item in delta)
                    {
                        delta_values += (item + " "); // add to string for reporting and output
                    }

                    Console.WriteLine();
                    Console.WriteLine("Values: " + output);
                    List<Point> intersection_points = new List<Point>(); // create list to keep track of intersections
                    List<int> intersection_values = new List<int>(); // create list to keep track of intersection values
                    foreach (var key in obj.Plots.Keys)
                    {
                        if (key.X == key.Y)
                        {
                            intersection_points.Add(key);
                            intersection_values.Add(obj.Plots[key]);
                        }
                        Console.WriteLine("Points {" + key.X + ", " + key.Y + "} => " + obj.Plots[key]);
                    }
                    Console.WriteLine();
                    Console.WriteLine("Delta: " + delta_values);
                    Console.WriteLine("Intersections: ");
                    for (int i = 0; i < intersection_points.Count; i++)
                    {
                        Console.WriteLine("Plot {" + intersection_points[i].X + ", " + intersection_points[i].Y + "} => (Value) " + intersection_values[i]);
                    }

                    string combo_list = "";

                    IEnumerable<IEnumerable<int>> combos = new List<IEnumerable<int>>();
                    if (!combo_hash.ContainsKey(delta_values)) // check it previously calculated
                    {
                        combos = Permutate<int>(delta).ToArray(); // create array of permutations/possibilities from the delta/missing values in data set
                        combo_hash.Add(delta_values, combos); // add permutations/possibilities to hashtable in memory to cache and speed up future calculations
                    }
                    else
                    {
                        combos = (IEnumerable<IEnumerable<int>>)combo_hash[delta_values]; // if previously calculated, use exsiting data 
                    }

                    obj.Combo_Hash_Key = delta_values; // Log hash key for permutations/possbilities to data set for future reference.

                    Console.WriteLine("Combos " + combos.Count()); // Output the count of possible solutions for single data set horizontal/vertical

                    foreach (var item in combos)
                    {
                        string combo = "";
                        foreach (var xx in item)
                        {
                            combo += (xx);
                        }

                        combo_list += (combo + Environment.NewLine); // create string of combos for reporting
                    }

                    // house cleaning 
                    intersection_points.Clear();
                    intersection_values.Clear();
                    tmpPlots.Clear();
                }

                Console.WriteLine();
                Console.WriteLine("---- Attempting To Solve ----");

                // Solve sets, using intersection point for common delta
                SolveSet(x); 

                Console.WriteLine();
            }
        }
```

```csharp
        private string PrintSets()
        {
            string output = "";
            foreach (var setKey in solution_sets.Keys)
            {
                Console.WriteLine("Printing Set Data ...");
                var set = solution_sets[setKey];
                foreach (var obj in set.Plots)
                {
                    Console.WriteLine(obj.Type.ToString());
                    string tmp = "";
                    foreach (var key in obj.Plots.Keys)
                    {
                        Console.WriteLine("Plot {" + key.X + ", " + key.Y + "} => " + obj.Plots[key]);
                        tmp += obj.Plots[key];
                    }
                    Console.WriteLine(tmp);
                }
                Console.WriteLine();
            }
            return output;
        }
```

```csharp
        private string PrintMatrix(bool console = true)
        {
            string output = "";

            Dictionary<int, int> vertSum = new Dictionary<int, int>();
            for (int y = 8; y > -1; y--)
            {
                int rowSum = 0;
                output += ((y) + " | ");
                for (int x = 0; x < 9; x++)
                {
                    int val = matrix[new Point(x, y)];
                    output += (val + "   ");
                    rowSum += val;
                    if (!vertSum.ContainsKey(x))
                    {
                        vertSum.Add(x, 0);
                    }
                    else
                    {
                        vertSum[x] += val;
                    }
                }
                output += " | " + rowSum;
                output += Environment.NewLine;
            }
            output += ("  ----------------------------------------");
            output += Environment.NewLine;
            output += ("    0   1   2   3   4   5   6   7   8");
            output += Environment.NewLine;
            output += ("   --- --- --- --- --- --- --- --- ---") + Environment.NewLine;

            output += " ";
            for (int i = 0; i < 9; i++)
            {
                output += ("  " + vertSum[i]);
            }

            if (console)
            {
                Console.WriteLine(output);
            }
            return output;
        }
```

```csharp
        private void Log()
        {
            // Write to data/output/{0}.sln.txt 
            string output = PrintMatrix();
            System.IO.File.WriteAllText(output_dir + System.IO.Path.GetFileName(file).Replace(".txt", ".sln.txt"), output);
            Console.WriteLine("-- Saved Solution Game " + System.IO.Path.GetFileName(file).Replace(".txt", ".sln.txt") + " --");

        }
```
## Example Outputs
 
