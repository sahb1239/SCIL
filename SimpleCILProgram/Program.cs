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

            Console.Write("Input number 2: ");

            if (!int.TryParse(Console.ReadLine(), out int input2))
            {
                Console.WriteLine("Not a valid number");
                return;
            }

            Console.WriteLine($"{input1} + {input2} = {input1 + input2}");
            Console.WriteLine($"{input1} - {input2} = {input1 - input2}");

            Console.WriteLine($"{input1} * {input2} = {input1 * input2}");
            Console.WriteLine($"{input1} / {input2} = {input1 / input2}");

            int branchedValue;
            if (input1 == 42)
            {
                branchedValue = input2;
            }
            else
            {
                branchedValue = 2;
            }

            Console.WriteLine(branchedValue);

            Console.WriteLine("Write some text: ");
            var directlyReadValue = Console.ReadLine();

            Console.WriteLine(directlyReadValue);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            try
            {
                throw new ArgumentException();
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Do something here");
            }
        }
    }
}
