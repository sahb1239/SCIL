using System;

namespace SimpleTaint
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            // This is tained from the user input
            Console.WriteLine(userInput);
        }
    }
}
