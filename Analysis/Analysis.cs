using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CoverageAnalysis
{
    public static class Analysis
    {
        static private string[] HandlerTypes => new[] { "CommandHandler", "QueryHandler", "AuditableCommandHandler" };
        static private string[] UnitTestAttributes => new[] { "Test", "SetUp" };
        static private string UnitTestType => "HandlerTest";

        public static async Task<AnalysisResult> ProcessAsync(Solution solution, string assemblyName)
        {
            int handlerCount = 0;
            int missingCount = 0;

            Console.WriteLine($"Retrieving the produced Compilation for {assemblyName}.");
            var project = solution.Projects.Where(p => p.AssemblyName == assemblyName).Single();
            var compilation = await project.GetCompilationAsync();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Starting to parse {compilation.SyntaxTrees.Count()} syntax trees.");
            foreach (var tree in compilation.SyntaxTrees)
            {
                var classDeclarations = tree.GetRoot().DescendantNodesAndSelf().Where(x => x.IsKind(SyntaxKind.ClassDeclaration));

                foreach (ClassDeclarationSyntax classDeclaration in classDeclarations)
                {
                    if (IsImplementingCommandOrQueryHandler(classDeclaration, tree, compilation))
                    {
                        handlerCount++;

                        if (IsCorrectlyCoveredByUnitTest(classDeclaration, tree, compilation, solution))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Success {assemblyName} {classDeclaration.Identifier.ValueText}");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Missing {assemblyName} {classDeclaration.Identifier.ValueText}");
                            missingCount++;
                        }
                    }
                }
            }

            if (handlerCount != 0 && missingCount > 0)
                return new AnalysisResult(false, handlerCount, missingCount, assemblyName);
            else
                return new AnalysisResult(true, handlerCount, missingCount, assemblyName);
        }

        private static bool IsImplementingCommandOrQueryHandler(ClassDeclarationSyntax classDeclaration, SyntaxTree tree, Compilation compilation)
        {
            // Checking if class is abstract
            if (classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword)))
                return false;

            // Checking if class implements a base type listed in HandlerTypes
            return IsBaseTypesPresent(classDeclaration, tree, compilation, HandlerTypes);
        }

        private static bool IsCorrectlyCoveredByUnitTest(ClassDeclarationSyntax classDeclaration, SyntaxTree tree, Compilation compilation, Solution solution)
        {
            var classSymbol = compilation.GetSemanticModel(tree).GetDeclaredSymbol(classDeclaration);
            var references = SymbolFinder.FindReferencesAsync(classSymbol, solution).Result;

            foreach (ReferencedSymbol reference in references)
            {
                foreach (var referenceLocation in reference.Locations)
                {
                    var location = referenceLocation.Location;
                    var node = location.SourceTree.GetRoot().FindNode(location.SourceSpan);
                    var methods = node.AncestorsAndSelf().Where(x => x.IsKind(SyntaxKind.MethodDeclaration));

                    if (methods.Any())
                    {
                        var referencedMethod = (MethodDeclarationSyntax)methods.First();
                        foreach (var attributeList in referencedMethod.AttributeLists)
                        {
                            foreach (var attribute in attributeList.Attributes)
                            {
                                if(UnitTestAttributes.Contains(attribute.Name.NormalizeWhitespace().ToFullString(), StringComparer.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    string[] baseTypes = new[] { $"{UnitTestType}<{classSymbol.Name}>" };
                    var classes = node.AncestorsAndSelf().Where(x => x.IsKind(SyntaxKind.ClassDeclaration));
                    if (classes.Any())
                    {
                        var referencedClass = (ClassDeclarationSyntax)classes.First();
                        if (IsBaseTypesPresent(referencedClass, baseTypes))
                            return true;
                    }
                }
            }

            return false;
        }

        private static bool IsBaseTypesPresent(ClassDeclarationSyntax classDeclaration, SyntaxTree tree, Compilation compilation, string[] baseTypes)
        {
            if (classDeclaration.BaseList == null || classDeclaration.BaseList.Types == null)
                return false;

            foreach (BaseTypeSyntax t in classDeclaration.BaseList.Types)
            {
                var typeInfo = compilation.GetSemanticModel(tree).GetTypeInfo(t.Type);
                if (baseTypes.Contains(typeInfo.Type.Name, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsBaseTypesPresent(ClassDeclarationSyntax classDeclaration, string[] baseTypes)
        {
            if (classDeclaration.BaseList == null || classDeclaration.BaseList.Types == null)
                return false;

            foreach (BaseTypeSyntax t in classDeclaration.BaseList.Types)
            {
                if (baseTypes.Contains(t.Type.ToString(), StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
