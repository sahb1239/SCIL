using System;

namespace OutParameters
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            var parsedInput = int.TryParse(userInput, out int result);

            Console.WriteLine(result);
        }
    }
}
