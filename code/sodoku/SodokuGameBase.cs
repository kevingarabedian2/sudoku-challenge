using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace sodoku
{
    public class SodokuGameBase
    {
        internal enum SetType
        {
            Horizontal,
            Vertical,
            Diagnal
        }
        internal class SetData
        {
            public Dictionary<Point, int> Plots = new Dictionary<Point, int>();
            public Point Intersection = new Point();
            public int IntersectionValue { get; set; }
            public string Combo_Hash_Key { get; set; }
            public List<int> Delta = new List<int>();
            public SetType Type { get; set; }

            public List<ComboSet> ComboSets = new List<ComboSet>();
        }
        public class ComboSet
        {
            public Dictionary<Point, int> Plots = new Dictionary<Point, int>();
            public List<int> Common_Delta = new List<int>();

        }
        internal class SodokuSet
        {
            public List<Point> H_Points = new List<Point>();
            public List<Point> V_Points = new List<Point>();
            public List<SetData> Plots = new List<SetData>();
        }

        private readonly List<int> allowed_values = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }); // Sudoku allowed values for data sets
        private readonly string output_dir = "../../../data/output/"; // result directory
        public Dictionary<Point, int> matrix = new Dictionary<Point, int>(); // stores original game data
        public List<Dictionary<Point, int>> possible_matrixs = new List<Dictionary<Point, int>>(); // stores possible solutions for full game
        public Dictionary<int, List<Point>> set_points = new Dictionary<int, List<Point>>();
        public List<int> remove_list = new List<int>();
        private Hashtable combo_hash = new Hashtable(); // cache permutation data
        private bool is_loaded = false; // did the input file load succesfully
        public string file = ""; // input file name

        private Dictionary<int, SodokuSet> solution_sets = new Dictionary<int, SodokuSet>(); // holds solved pair data sets, partial solutions

        public List<int> CalculateCommonDelta(List<Point> h, List<Point> v)
        {
            List<int> tmp1 = new List<int>();
            List<int> tmp2 = new List<int>();

            foreach (var p in h)
            {
                tmp1.Add(matrix[p]);
            }
            foreach (var p in v)
            {
                tmp2.Add(matrix[p]);
            }
            return tmp1.Intersect(tmp2).ToList();
        }

        private class EnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
        {
            public bool Equals(IEnumerable<T> a, IEnumerable<T> b)
            {
                return a.SequenceEqual(b);
            }

            public int GetHashCode(IEnumerable<T> t)
            {
                return t.Take(1).Aggregate(0, (a, x) => a ^ x.GetHashCode());
            }
        }
        public IEnumerable<IEnumerable<T>> Permutate<T>(IEnumerable<T> source)
        {
            var xs = source.ToArray();
            return
                xs.Length == 1
                    ? new[] { xs }
                    : (
                        from n in Enumerable.Range(0, xs.Length)
                        let cs = xs.Skip(n).Take(1)
                        let dss = Permutate<T>(xs.Take(n).Concat(xs.Skip(n + 1)))
                        from ds in dss
                        select cs.Concat(ds)
                    ).Distinct(new EnumerableEqualityComparer<T>());
        }
        public void Print(int x, int y)
        {
            Console.WriteLine("{ " + x + ", " + y + " } => " + matrix[new Point(x, y)]);
        }
        public void Solve()
        {
            for (int x = 0; x < 9; x++) // Traverse diag to obtain all intersecting sets
            {
                solution_sets.Add(x, GenerateSetData(x, x)); // Create sets of horizontal and vertical pairs
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
                    delta.Sort();
                    obj.Delta = delta.ToList();

                    string delta_values = "";
                    foreach (var item in delta)
                    {
                        delta_values += (item + " "); // add to string for reporting and output
                    }

                    //Console.WriteLine();
                    // Console.WriteLine("Values: " + output);
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
                    // Console.WriteLine(combo_list);
                    // house cleaning 
                    intersection_points.Clear();
                    intersection_values.Clear();
                    tmpPlots.Clear();
                }
            }

            Console.WriteLine();
            Console.WriteLine("---- Attempting To Solve ----");
            Console.WriteLine();

            // Solve sets, using intersection point for common delta
            List<List<ComboSet>> Horizontal = new List<List<ComboSet>>();
            List<List<ComboSet>> Vertical = new List<List<ComboSet>>();
            for (int x = 0; x < 9; x++) // Traverse diag to obtain all intersecting sets
            {
                var tmp = SolveSet(x);
                Horizontal.Add(tmp[0].ToList());
                Vertical.Add(tmp[1].ToList());
                //Console.ReadLine();
            }
            List<Dictionary<Point, int>> solutions = new List<Dictionary<Point, int>>();
            List<Dictionary<Point, int>> solutions2 = new List<Dictionary<Point, int>>();
            for (int x = 0; x < 9; x++) // Traverse diag to obtain all intersecting sets
            {
                foreach (var set_data in Horizontal)
                {
                    foreach (var set in set_data)
                    {
                        var possible = new Dictionary<Point, int>(matrix);
                        foreach (Point point in set.Plots.Keys)
                        {
                            if (possible[point] == 0)
                            {
                                possible[point] = set.Plots[point];
                            }
                        }
                        solutions.Add(new Dictionary<Point, int>(possible));
                    }
                }

                foreach (var set_data in Vertical)
                {
                    foreach (var set in set_data)
                    {
                        var possible = new Dictionary<Point, int>(matrix);
                        foreach (Point point in set.Plots.Keys)
                        {
                            if (possible[point] == 0)
                            {
                                possible[point] = set.Plots[point];
                            }
                        }
                        solutions2.Add(new Dictionary<Point, int>(possible));
                    }
                }
                //second loop
                foreach (var set_data in Vertical)
                {
                    foreach (var set in set_data)
                    {
                        var possible = new Dictionary<Point, int>(matrix);
                        foreach (Point point in set.Plots.Keys)
                        {
                            if (possible[point] == 0)
                            {
                                possible[point] = set.Plots[point];
                            }
                        }
                        solutions2.Add(new Dictionary<Point, int>(possible));
                    }
                }

                foreach (var possible in solutions2)
                {
                    if (ValidateMatrix(possible))
                    {
                        PrintMatrix(possible, true);
                        string output = PrintMatrix(matrix, true);
                        System.IO.File.AppendAllText(output_dir + "debug.sln.txt", output);
                    }
                    else
                    {
                        //Console.WriteLine("Failed validation.");
                    }
                }


            }

            var tst = "";


        }
        public List<List<ComboSet>> SolveSet(int key)
        {
            var xdata = solution_sets[key];
            List<ComboSet> h_sets = new List<ComboSet>();
            List<ComboSet> v_sets = new List<ComboSet>();

            var h_data = xdata.Plots.Where(x => x.Type == SetType.Horizontal).ToList();
            var v_data = xdata.Plots.Where(x => x.Type == SetType.Vertical).ToList();

            foreach (var h_set in h_data)
            {
                foreach (var obj in (IEnumerable<IEnumerable<int>>)combo_hash[h_set.Combo_Hash_Key])
                {
                    ComboSet comboset = new ComboSet();
                    comboset.Plots = new Dictionary<Point, int>(h_set.Plots);

                    int index = 0;
                    var combo = obj.ToArray();

                    foreach (var point in h_set.Plots.Keys.ToList())
                    {
                        if (h_set.Plots[point] == 0)
                        {
                            comboset.Common_Delta = CalculateCommonDelta(h_set.Plots.Keys.ToList(), v_data[0].Plots.Keys.ToList());
                            comboset.Common_Delta.Remove(0);
                            comboset.Plots[point] = combo[index];
                            index++;
                        }
                    }
                    h_sets.Add(comboset);
                }
            }

            foreach (var v_set in v_data)
            {
                foreach (var obj in (IEnumerable<IEnumerable<int>>)combo_hash[v_set.Combo_Hash_Key])
                {
                    ComboSet comboset = new ComboSet();
                    comboset.Plots = new Dictionary<Point, int>(v_set.Plots);

                    int index = 0;
                    var combo = obj.ToArray();
                    foreach (var point in v_set.Plots.Keys.ToList())
                    {
                        if (v_set.Plots[point] == 0)
                        {
                            comboset.Common_Delta = CalculateCommonDelta(h_data[0].Plots.Keys.ToList(), v_set.Plots.Keys.ToList());
                            comboset.Common_Delta.Remove(0);
                            comboset.Plots[point] = combo[index];
                            index++;
                        }
                    }
                    v_sets.Add(comboset);
                }
            }

            return new List<List<ComboSet>>
            {
                h_sets,
                v_sets
            };



            /*

           bool isFirst = false;
           var possible = new Dictionary<Point, int>(matrix);
           foreach (var set in h_validated_sets)
           {
               if (possible_matrixs.Count == 0)
               {
                   isFirst = true;
               }
               foreach (var plot in set.Plots.Keys)
               {
                   //if (plot.X == plot.Y)
                   //{
                   foreach (var set2 in v_validated_sets)
                   {
                       foreach (var plot2 in set2.Plots.Keys)
                       {
                           if (plot.X == plot.Y && plot2.X == plot2.Y) // Check for intersection
                           {
                               int set_a_val = set.Plots[plot];
                               int set_b_val = set2.Plots[plot2];

                               //  if (isFirst) // Load first possibilites to check next
                               // {
                               //possible = new Dictionary<Point, int>(matrix);
                               foreach (var p in set.Plots.Keys) // horizontal
                               {
                                   possible[p] = set.Plots[p];
                                   //Print(p.X, p.Y);
                               }

                               foreach (var p in set2.Plots.Keys) // vertical 
                               {
                                   possible[p] = set2.Plots[p];
                                   //Print(p.X, p.Y);
                               }

                               //possible_matrixs.Add(new Dictionary<Point, int>(possible));
                               /* Console.WriteLine(" Original ");
                                PrintMatrix(matrix, true);
                                Console.WriteLine(" Suggested ");*/
            //PrintMatrix(possible, true);
            //}
            //else
            //{
            // for (int i = 0; i < possible_matrixs.Count; i++)
            // {
            /*                     bool valid = true;
                                 //possible = new Dictionary<Point, int>(possible_matrixs[0]);
                                 // check if 0, or if have value that it matches, else bounce key
                                 foreach (var p in set.Plots.Keys)
                                 {
                                     if (valid)
                                     {
                                         if (possible[p] == 0)
                                         {
                                             if (set.Plots.ContainsKey(p))
                                             {
                                                 possible[p] = set.Plots[p];
                                                /* if (!set_points.ContainsKey(i))
                                                 {
                                                     set_points.Add(i, new List<Point>());
                                                 }
                                                 if (!set_points[i].Contains(p))
                                                 {
                                                     set_points[i].Add(p);
                                                 }*/
            /*                               }
                                       }
                                       else
                                       {
                                           if (set.Plots.ContainsKey(p))
                                           {
                                               if (possible[p] != set.Plots[p])
                                               {
                                                   valid = false;
                                               }
                                           }
                                       }
                                   }
                               }
                               foreach (var p in set2.Plots.Keys)
                               {
                                   if (valid)
                                   {
                                       if (possible[p] == 0)
                                       {
                                           if (set2.Plots.ContainsKey(p))
                                           {
                                               possible[p] = set2.Plots[p];
                                           }
                                       }
                                       else
                                       {
                                           if (set2.Plots.ContainsKey(p))
                                           {
                                               if (possible[p] != set2.Plots[p])
                                               {
                                                   valid = false;
                                               }
                                           }
                                       }
                                   }
                               }

                               if (valid)
                               {
                                  // possible_matrixs[i] = new Dictionary<Point, int>(possible);
                                   PrintMatrix(possible, true);
                               }
                               else
                               {
                                   /*if (!remove_list.Contains(i))
                                   {
                                       remove_list.Add(i);
                                   }*/
            //                 }
            //           }
            //}

            //}
            //  }
            // }
            //}

            //}
            // }
            /* Console.WriteLine();
             Console.WriteLine("Possible solutions for set " + key + " : " + possible_matrixs.Count);
             for (int i = 0; i < possible_matrixs.Count; i++)
             {
                 bool valid = true;
                 foreach (var p in possible_matrixs[i])
                 {
                     if (p.Value == 0)
                     {
                         valid = false;
                     }
                 }


                 PrintMatrix(possible_matrixs[i], true);

             }*/
        }

        public void Start()
        {
            Console.WriteLine();
            Console.WriteLine("-- Solving Game " + System.IO.Path.GetFileName(file) + " --");
            is_loaded = Load(file);
            if (is_loaded)
            {
                Solve();
                //PrintSets();
                Log();
            }
            Console.WriteLine();
        }

        private SodokuSet GenerateSetData(int x, int y)
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
            sodokuSet.V_Points = setData.Plots.Keys.ToList();
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
            sodokuSet.H_Points = setData.Plots.Keys.ToList();
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
        private void Log()
        {
            // Write to data/output/{0}.sln.txt 
            string output = PrintMatrix(matrix, true);
            System.IO.File.WriteAllText(output_dir + System.IO.Path.GetFileName(file).Replace(".txt", ".sln.txt"), output);
            Console.WriteLine("-- Saved Solution Game " + System.IO.Path.GetFileName(file).Replace(".txt", ".sln.txt") + " --");

        }

        /* private string PrintSolutions()
         {
             string output = "";
             for (int i = 1; i < 9; i++)
             {
                 Console.WriteLine("Printing Solution Set Data ...");
                 var set_a = solution_sets[i - 1];
                 var set_b = solution_sets[i];

                 foreach (var obj in set_a.SolutionSets[0])
                 {
                     foreach (var point_a in obj.Plots.Keys)
                     {
                         foreach (var obj2 in set_b.SolutionSets[1])
                         {
                             foreach (var point_b in obj2.Plots.Keys)
                             {
                                 if (point_a == point_b && obj.Plots[point_a] == obj2.Plots[point_b])
                                 {
                                     // set good
                                     Console.WriteLine("Horizontal Plot {" + point_a.X + ", " + point_a.Y + "} => " + obj.Plots[point_a]);
                                     Console.WriteLine();
                                 }
                             }
                         }
                     }
                 }
                 Console.WriteLine();
             }
             return output;
         }*/

        private string PrintMatrix(Dictionary<Point, int> the_matrix, bool console = true)
        {
            string output = "";

            Dictionary<int, int> vertSum = new Dictionary<int, int>();
            for (int y = 8; y > -1; y--)
            {
                int rowSum = 0;
                output += ((y) + " | ");
                for (int x = 0; x < 9; x++)
                {
                    int val = the_matrix[new Point(x, y)];
                    output += (val + "   ");
                    rowSum += val;
                    if (!vertSum.ContainsKey(x))
                    {
                        vertSum.Add(x, 0);
                    }
                    vertSum[x] += val;
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

        private bool ValidateMatrix(Dictionary<Point, int> the_matrix)
        {
            Dictionary<int, int> vertSum = new Dictionary<int, int>();
            int total_sum = 0;
            for (int y = 8; y > -1; y--)
            {
                int rowSum = 0;
                for (int x = 0; x < 9; x++)
                {
                    int val = the_matrix[new Point(x, y)];
                    rowSum += val;
                    if (!vertSum.ContainsKey(x))
                    {
                        vertSum.Add(x, 0);
                    }
                    vertSum[x] += val;
                    total_sum += val;
                }
                if (total_sum > 0)
                {
                    return true;
                }
                /*if(rowSum != 45)
                {
                    return false;
                }*/
            }

            for (int i = 0; i < 9; i++)
            {
                /*if(vertSum[i] != 45)
                {
                    return false;
                }*/
            }

            return true;
        }


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
                // Console.WriteLine();
            }
            return output;
        }
    }
}