using System;

namespace SimpleCILProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Input number 1: ");

            if (!int.TryParse(Console.ReadLine(), out int input1))
            {
                Console.WriteLine("Not a valid number");
                return;
            }

            if (input1 == 10)
            {
                input1 = 20;
            }
            else if (input1 == 20)
            {
                input1 = 30;
            }
            else
            {
                input1 = 0;
            }

            Console.WriteLine(input1);
        }
    }
}