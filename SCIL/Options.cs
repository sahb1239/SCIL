using CommandLine;

namespace SCIL
{
    class Options
    {
        [Value(0, MetaName = "ApkFile", Required = true, HelpText = "Apk files to be processed.")]
        public string ApkFile { get; set; }

        [Value(1, MetaName = "OutputPath", Required = true, HelpText = "Output path")]
        public string OutputPath { get; set; }
    }
}