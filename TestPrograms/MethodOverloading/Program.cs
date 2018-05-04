using System;

namespace MethodOverloading
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            var funnyTextMaker = new FunnyTextMaker();

            var myFunnyText = funnyTextMaker.AddLOL(5, userInput);

            Console.WriteLine(myFunnyText);
        }
    }

    public class FunnyTextMaker
    {
        public string AddLOL()
        {
            return "LOL";
        }

        public string AddLOL(string s)
        {
            return s + " LOL";
        }

        public string AddLOL(int numberOfLOLs, string s)
        {
            var result = s;

            for (int i = 0; i < numberOfLOLs; i++)
            {
                result = result + " LOL";
            }

            return result;
        }
    }
}
