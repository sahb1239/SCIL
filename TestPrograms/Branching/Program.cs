using System;

namespace Branching
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            if (userInput == "something")
                Console.WriteLine(userInput);
            else
                Console.WriteLine("Not tainted");
        }
    }
}
