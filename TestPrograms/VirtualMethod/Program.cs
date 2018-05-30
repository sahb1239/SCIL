using System;

namespace VirtualMethod
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = Console.ReadLine();
            GetA().Test(input);
        }

        static A GetA()
        {
            return new B();
        }
    }

    class A
    {
        public virtual string Test(string str)
        {
            return str;
        }
    }

    class B : A
    {
        public override string Test(string str)
        {
            Console.WriteLine(str);
            return base.Test(str);
        }
    }
}
