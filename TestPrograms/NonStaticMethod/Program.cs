using System;

namespace NonStaticMethod
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            var funnyTextMaker = new FunnyTextMaker();

            var myFunnyText = funnyTextMaker.AddLOL(userInput);

            Console.WriteLine(myFunnyText);
        }
    }

    public class FunnyTextMaker
    {
        public string AddLOL(string s)
        {
            return s + " LOL";
        }
    }
}
