using System;

namespace sodoku
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SodokuGame Game1 = new SodokuGame("../../../data/input/puzzle1.txt");
            Game1.Start();

            //SodokuGame Game2 = new SodokuGame("../../../data/input/puzzle2.txt");
            //Game2.Start();

            //SodokuGame Game3 = new SodokuGame("../../../data/input/puzzle3.txt");
            //Game3.Start();

            //SodokuGame Game4 = new SodokuGame("../../../data/input/puzzle4.txt");
            //Game4.Start();

            //SodokuGame Game5 = new SodokuGame("../../../data/input/puzzle5.txt");
            //Game5.Start();



            Console.WriteLine();
            Console.WriteLine("Hello World!");
        }
    }
}
