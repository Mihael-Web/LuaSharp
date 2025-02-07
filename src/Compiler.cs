// Imports //
using LuaSharp.src.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Namespace //
namespace LuaSharp.src
{
    public static class CompilerLS
    {
        public static string TraverseSubTokens(List<object> tokenList)
        {
            foreach (var subToken in tokenList)
            {
                // Still todo
            }

            return string.Empty;
        }

        public static string TraverseAndBuildSourceFromTokenTree(Dictionary<string, object> lsTokenTree)
        {
            foreach (var token in lsTokenTree)
            {
                if (token.Value is List<object> tokenList)
                {
                    TraverseSubTokens(tokenList);
                }
                else
                {
                    continue;
                }
            }

            return string.Empty;
        }

        public static void BuildFile(string file)
        {
            string sourceCode = File.ReadAllText(file);
            if (sourceCode == null || sourceCode == string.Empty)
            {
                Console.WriteLine("[LuaShrp] [Compilation] Failed to get source code for file" + file + "due to it being either empty or there is generally an internal or external error.");
                return;
            }

            CompilationUnitSyntax unitSyntaxResult = MimicLexer.GetCompilationUnitRootForSource(sourceCode);
            Dictionary<string, object> lsTokenTree = MimicLexer.TurnToDynamicTokenTree(unitSyntaxResult);
            string luauSourceCode = TraverseAndBuildSourceFromTokenTree(lsTokenTree);
        }

        public static void AttemptBuild(string sourceDirectory, string outDirectory)
        {
            foreach (var file in Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories))
            {
                BuildFile(file);
            }
        }
    }
}
