using System.Collections.Generic;
using SCIL.Logger;

namespace SCIL
{
    interface IInstructionAnalyzer
    {
        void Reset();
        IEnumerable<string> GetOutput();
    }
}