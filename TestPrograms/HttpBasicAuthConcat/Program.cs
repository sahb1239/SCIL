using System;

namespace HttpBasicAuthConcat
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "url.dk";

            var fullUrl = "http://user:pass@" + url;

            Console.WriteLine(fullUrl);
        }
    }
}
