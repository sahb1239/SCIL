using System;

namespace RefParameters
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            AddLOL(ref userInput);

            Console.WriteLine(userInput);
        }

        static void AddLOL(ref string s)
        {
            s = s + " LOL";
        }
    }
}
