using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCIL.Flix
{
    public interface IFlixExecutor : IDisposable
    {
        Task Execute(IEnumerable<string> files);
    }
}