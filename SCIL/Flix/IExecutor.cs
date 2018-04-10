using System;
using System.Collections.Generic;

namespace SCIL.Flix
{
    internal interface IExecutor : IDisposable
    {
        void Execute(IEnumerable<string> files, params string[] args);
    }
}