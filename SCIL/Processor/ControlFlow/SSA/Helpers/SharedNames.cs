using System.Collections.Generic;
using System.Linq;

namespace SCIL.Processor.ControlFlow.SSA.Helpers
{
    public class SharedNames
    {
        private readonly List<List<string>> _names = new List<List<string>>();

        public string GetNewName(int index)
        {
            // Add extra index list if it does not exists
            while (_names.Count <= index)
            {
                _names.Add(new List<string>());
            }

            // Get index list
            var indexList = _names[index];
            var indexName = $"{index}_{indexList.Count}";
            indexList.Add(indexName);

            return indexName;
        }

        public string GetCurrentName(int index)
        {
            // Add extra index list if it does not exists
            while (_names.Count <= index)
            {
                _names.Add(new List<string>());
            }

            // Get index list
            var indexList = _names[index];

            // Can be not assigned yet (for example if stack has not been used yet)
            // For example System.Net.TimerThread.OnDomainUnload
            /*
             *
                .try
                {
                  // [75 9 - 75 38]
                  IL_0000: call         void System.Net.TimerThread::StopTimerThread()

                  IL_0005: leave.s      IL_000a
                } // end of .try
                catch [mscorlib]System.Object
                {

                  // [77 7 - 77 12]
                  IL_0007: pop          

                  IL_0008: leave.s      IL_000a
                } // end of catch
             */
            // IL_0007 is not assigned to any parent variable and therefore will not be created
            return indexList.LastOrDefault() ?? "NIL";
        }
    }
}