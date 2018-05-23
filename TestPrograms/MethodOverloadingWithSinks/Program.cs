using System;

namespace MethodOverloadingWithSinks
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            var funnyTextPrinter = new FunnyTextPrinter();

            funnyTextPrinter.Print(5, userInput);
        }
    }

    public class FunnyTextPrinter
    {
        public void Print()
        {
            Console.WriteLine("LOL");
        }

        public void Print(string s)
        {
            Console.WriteLine(s + " LOL");
        }

        public void Print(int numberOfLOLs, string s)
        {
            var result = s;

            for (int i = 0; i < numberOfLOLs; i++)
            {
                result = result + " LOL";
            }

            Console.WriteLine("No use of tainted values");
        }
    }
}
