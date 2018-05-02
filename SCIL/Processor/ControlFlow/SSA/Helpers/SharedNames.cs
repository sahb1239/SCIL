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
            return indexList.Last();
        }
    }
}