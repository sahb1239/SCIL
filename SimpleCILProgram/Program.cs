using System;

namespace SimpleCILProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var nasty = Console.ReadLine();

            if (nasty == "john")
                Console.WriteLine(nasty);
            else
                Console.WriteLine("not nasty");
        }
    }
}