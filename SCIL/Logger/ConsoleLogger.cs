using System;

namespace SCIL.Logger
{
    class ConsoleLogger : ILogger
    {
        public bool Verbose { get; }
        public bool WaitForInput { get; }

        public ConsoleLogger(bool verbose, bool waitForInput)
        {
            Verbose = verbose;
            WaitForInput = waitForInput;
        }
        
        public void Log(string message) => Log(message, false);

        public void Log(string message, bool verbose)
        {
            if (!verbose || Verbose)
            {
                Console.WriteLine(message);
            }
        }

        public void Wait()
        {
            if (!WaitForInput)
                return;

            Console.Write("Press any key to continue... ");
            Console.ReadKey();
            Console.WriteLine();
        }
    }
}