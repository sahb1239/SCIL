using System;
using System.Collections.Generic;

namespace SCIL.Flix
{
    public interface IFlixExecutor : IDisposable
    {
        void Execute(IEnumerable<string> files);
    }
}