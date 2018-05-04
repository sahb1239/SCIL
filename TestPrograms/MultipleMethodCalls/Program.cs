using System;

namespace MultipleMethodCalls
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            var userInputWithLol = AddLOL(userInput);

            Console.WriteLine(userInputWithLol);
        }

        static string AddLOL(string s)
        {
            return AddHaha(s + " LOL");
        }

        static string AddHaha(string s)
        {
            return s + " haha";
        }
    }
}
