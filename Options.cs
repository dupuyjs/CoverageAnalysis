using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoverageAnalysis
{
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('s', "solution", Required = true, HelpText = "Set solution path.")]
        public string SolutionPath { get; set; }

        [Option('a', "assemblies", Required = true, HelpText = "Set assembly names.")]
        public IEnumerable<string> AssemblyNames { get; set; }
    }
}
