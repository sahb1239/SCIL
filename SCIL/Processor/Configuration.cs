using System;
using System.Collections.Generic;
using System.Linq;

namespace SCIL
{
    public class Configuration
    {
        public Configuration(IEnumerable<string> excludedModules, string outputPath)
        {
            ExcludedModules = excludedModules?.ToList() ?? throw new ArgumentNullException(nameof(excludedModules));
            OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
        }

        public IReadOnlyCollection<string> ExcludedModules { get; }
        public string OutputPath { get; }
    }
}