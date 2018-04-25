using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCIL.Processor
{
    public static class MethodReferenceExtensions
    {
        public static string NameOnly(this MethodReference value)
        {
            var splitted = value.FullName.Split(' ');
            return splitted[splitted.Length - 1];
        }
    }
}
