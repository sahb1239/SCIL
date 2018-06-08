using System;

namespace HttpBasicAuthSimple
{
    class Program
    {
        static void Main(string[] args)
        {
            // Loading a secret string (http basic auth)
            var secret = "http://user:pass@url.dk";

            Console.WriteLine(secret);
        }
    }
}
