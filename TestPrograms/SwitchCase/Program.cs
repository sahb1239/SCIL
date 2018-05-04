using System;

namespace SwitchCase
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = (int)Console.ReadKey().KeyChar;

            switch (userInput)
            {
                // The compiler should use the Switch instruction since there are consecutive
                case 1:
                    Console.WriteLine(userInput); break;
                case 2:
                    Console.WriteLine(userInput); break;
                case 3:
                    Console.WriteLine(userInput); break;
                case 4:
                    Console.WriteLine(userInput); break;
                case 5:
                    Console.WriteLine(userInput); break;
                case 6:
                    Console.WriteLine(userInput); break;
            }
        }
    }
}
