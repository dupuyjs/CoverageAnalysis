using CommandLine;
using CoverageAnalysis.Rules;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoverageAnalysis
{
    class Program
    {
        static int Main(string[] args)
        { 
            ExitCode exitCode = ExitCode.Success;
            Task<ExitCode> mainTask = null;

            Settings.Init();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    mainTask = LoadSolutionAndStartAnalysis(o.SolutionPath, o.AssemblyNames);
                })
                .WithNotParsed<Options>(a =>
                {
                    exitCode = ExitCode.BadArgument;
                });

            if( mainTask!=null )
            {
                Task.WaitAll(mainTask);
                exitCode = mainTask.Result;
            }

            return (int)exitCode;
        }

        private static async Task<ExitCode> LoadSolutionAndStartAnalysis(string solutionPath, IEnumerable<string> assemblyNames)
        {
            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                // If there is only one instance of MSBuild on this machine, set that as the one to use.
                ? visualStudioInstances[0]
                // Handle selecting the version of MSBuild you want to use.
                : SelectVisualStudioInstance(visualStudioInstances);

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                Console.WriteLine($"Loading solution '{solutionPath}'");
                // Attach progress reporter so we print projects as they are loaded.
                var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
                Console.WriteLine($"Finished loading solution '{solutionPath}'");

                var tasks = new List<Task<AnalysisResult>>();
                foreach (var assemblyName in assemblyNames)
                {
                    tasks.Add(Analysis.ProcessAsync(solution, assemblyName));
                }

                Task.WaitAll(tasks.ToArray());
                Console.WriteLine();

                foreach (var task in tasks)
                {
                    var result = task.Result;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"{result.AssemblyName} contains {result.HandlerCount} handlers with {result.MissingCount} missing unit tests.");
                }

                foreach (var task in tasks)
                {
                    var result = task.Result;
                    if (result.IsSuccess == false)
                    {
                        return ExitCode.MissingTest;
                    }
                }
            }

            return (int)ExitCode.Success;
        }

        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }
    }
}
