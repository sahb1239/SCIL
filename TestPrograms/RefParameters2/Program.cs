using System;

namespace RefParameters2
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = "My name is: ";

            AddUserInput(ref result);

            Console.WriteLine(result);
        }

        static void AddUserInput(ref string s)
        {
            var name = Console.ReadLine();

            s = s + name;
        }
    }
}
