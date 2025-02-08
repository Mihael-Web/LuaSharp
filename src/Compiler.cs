// Imports //
using LuaSharp.src.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Namespace //
namespace LuaSharp.src
{
    public static class CompilerLS
    {

        /// <summary>
        /// Builds a file from source to Luau.
        /// </summary>
        /// <param name="file">The file to build to luau.</param>
        /// <param name="outDir">The output directory path.</param>
        public static void BuildFile(string file, string outDir)
        {
            string sourceCode = File.ReadAllText(file);
            if (sourceCode == null || sourceCode == string.Empty)
            {
                Console.WriteLine("[LuaShrp] [Compilation] Failed to get source code for file" + file + "due to it being either empty or there is generally an internal or external error.");
                return;
            }

            SyntaxNode syntaxTreeRoot = ASTFetcher.GetSyntaxTreeRoot(sourceCode);
            ASTFetcher.GetNamespaceDeclarations(syntaxTreeRoot);
        }

        /// <summary>
        /// Attempts to build a source directory to output directory.
        /// </summary>
        /// <param name="sourceDirectory">The path to the source directory.</param>
        /// <param name="outDirectory">The path to the output directory.</param>
        public static void AttemptBuild(string sourceDirectory, string outDirectory, string mainDirectory)
        {
            // First we check if out directory exists, and if not we create the directory. //
            string outDirCombined = mainDirectory + "/" + outDirectory;
            if (!Directory.Exists(outDirCombined))
            {
                Directory.CreateDirectory(outDirCombined);
            }

            // Now, we can build all the files within the source directory //
            foreach (var file in Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories))
            {
                try
                {
                    BuildFile(file, outDirCombined);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LuaShrp] [Compilation] Error compiling file {file}: {ex.Message}");
                }
            }
        }
    }
}
