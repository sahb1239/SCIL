using System;
using System.Collections.Generic;
using System.Linq;

namespace SCIL
{
    public class Configuration
    {
        public Configuration(IEnumerable<string> excludedModules, string outputPath, bool @async, IEnumerable<string> javaArgs, IEnumerable<string> flixArgs, bool showFlixWindow)
        {
            ExcludedModules = excludedModules?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(excludedModules));
            OutputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
            Async = async;
            JavaArgs = javaArgs?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(javaArgs));
            FlixArgs = flixArgs?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(flixArgs));
            ShowFlixWindow = showFlixWindow;
        }

        public IReadOnlyCollection<string> ExcludedModules { get; }
        public IReadOnlyCollection<string> JavaArgs { get; }
        public IReadOnlyCollection<string> FlixArgs { get; }
        public string OutputPath { get; }
        public bool Async { get; }
        public bool ShowFlixWindow { get; }
    }
}