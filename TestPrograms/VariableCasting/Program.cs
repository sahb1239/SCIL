using System;

namespace VariableCasting
{
    class Program
    {
        static void Main(string[] args)
        {
            char userInput = Console.ReadKey().KeyChar;

            int userInputAsInt = (int)userInput;

            Console.WriteLine(userInputAsInt);
        }
    }
}
