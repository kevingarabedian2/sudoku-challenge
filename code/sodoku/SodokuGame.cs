using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace sodoku
{  
    public class SodokuGame : SodokuGameBase
    {
        // Example Board Matrix
        /*
        8 |	1	2	3	4	5	6	7	8	9
          |
        7 |	1	2	3	4	5	6	7	8	9
          |
        6 |	1	2	3	4	5	6	7	8	9
          |
        5 |	1	2	3	4	5	6	7	8	9
          |
        4 |	1	2	3	4	5	6	7	8	9
          |
        3 |	1	2	3	4	5	6	7	8	9
          |
        2 |	1	2	3	4	5	6	7	8	9
          |
        1 |	1	2	3	4	5	6	7	8	9	
          |
        0 |	1	2	3	4	5	6	7	8	9
           ----------------------------------
            0	1	2	3	4	5	6	7	8

        */
        
        public SodokuGame(string filename)
        {
            file = filename;
            matrix = new Dictionary<Point, int>();
        }
    }
}