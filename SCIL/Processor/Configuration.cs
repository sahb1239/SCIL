using System;
using System.Collections.Generic;
using System.Linq;

namespace SCIL
{
    public class Configuration
    {
        public Configuration(IEnumerable<string> excludedModules, string outputPath, bool @async)
        {
            ExcludedModules = excludedModules?.ToList() ?? throw new ArgumentNullException(nameof(excludedModules));
            OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
            Async = async;
        }

        public IReadOnlyCollection<string> ExcludedModules { get; }
        public string OutputPath { get; }
        public bool Async { get; }
    }
}