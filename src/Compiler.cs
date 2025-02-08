// Imports //
using LuaSharp.src.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

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

            string luaCode = CompilerLSConverter.ConvertCSharpToLuau(sourceCode);
            string outputFilePath = Path.Combine(outDir, Path.GetFileName(file));

            string outputDir = Path.GetDirectoryName(outputFilePath) ?? string.Empty;
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            File.WriteAllText(outputFilePath, luaCode);
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

    /// <summary>
    /// A class for converting C# code to Luau (Roblox scripting language) using an Abstract Syntax Tree (AST).
    /// </summary>
    public static class CompilerLSConverter
    {
        private static readonly Dictionary<string, Action<Dictionary<string, object>, StringBuilder, int>> NodeHandlers;

        static CompilerLSConverter()
        {
            // Initialize node handlers
            NodeHandlers = new Dictionary<string, Action<Dictionary<string, object>, StringBuilder, int>>
            {
                { "NamespaceDeclarationSyntax", HandleNamespaceDeclaration },
                { "ClassDeclarationSyntax", HandleClassDeclaration },
                { "MethodDeclarationSyntax", HandleMethodDeclaration }
            };
        }

        /// <summary>
        /// Converts C# source code to Luau code.
        /// </summary>
        /// <param name="sourceCode">The C# source code.</param>
        /// <returns>Luau code as a string.</returns>
        public static string ConvertCSharpToLuau(string sourceCode)
        {
            var syntaxTreeRoot = ASTFetcher.GetSyntaxTreeRoot(sourceCode);
            var fullAST = ASTParser.GetFullAST(syntaxTreeRoot);

            var luauCode = new StringBuilder();
            foreach (var node in fullAST)
            {
                ProcessNode(node, luauCode, 0);
            }

            return luauCode.ToString();
        }

        /// <summary>
        /// Recursively processes an AST node and generates the corresponding Luau code.
        /// </summary>
        /// <param name="node">The AST node.</param>
        /// <param name="luauCode">The StringBuilder to append the generated Luau code.</param>
        /// <param name="indentLevel">The current indentation level.</param>
        private static void ProcessNode(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);

            // Check if there's a handler for the node type and execute it
            var nodeType = node["NodeType"]?.ToString();
            if (nodeType != null && NodeHandlers.TryGetValue(nodeType, out Action<Dictionary<string, object>, StringBuilder, int>? value))
            {
                value(node, luauCode, indentLevel);
            }
            else
            {
                // If no specific handler, process the node recursively
                ProcessGenericNode(node, luauCode, indentLevel);
            }
        }

        /// <summary>
        /// Generic handler to process any node not explicitly defined in NodeHandlers.
        /// </summary>
        /// <param name="node">The AST node.</param>
        /// <param name="luauCode">The StringBuilder to append the generated Luau code.</param>
        /// <param name="indentLevel">The current indentation level.</param>
        private static void ProcessGenericNode(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var nodeRepresentation = node["NodeType"].ToString();

            // Just output the node type and its children (if any)
            luauCode.AppendLine($"{indent}-- {nodeRepresentation}");

            ProcessChildren(node, luauCode, indentLevel);
        }

        /// <summary>
        /// Processes an individual Namespace Declaration node.
        /// </summary>
        private static void HandleNamespaceDeclaration(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var namespaceName = node.TryGetValue("Name", out object? value) ? value.ToString() : "";
            luauCode.AppendLine($"{indent}-- Namespace: {namespaceName}");

            ProcessChildren(node, luauCode, indentLevel);
        }

        /// <summary>
        /// Processes an individual Class Declaration node.
        /// </summary>
        private static void HandleClassDeclaration(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var className = node.TryGetValue("Name", out object? value) ? value.ToString() : "";

            luauCode.AppendLine($"{indent}-- Class: {className}");
            luauCode.AppendLine($"{indent}local {className} = {{}}");

            ProcessChildren(node, luauCode, indentLevel + 1);
        }

        /// <summary>
        /// Processes an individual Method Declaration node.
        /// </summary>
        private static void HandleMethodDeclaration(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var methodName = node.TryGetValue("Name", out object? value) ? value.ToString() : "";
            var returnType = node.TryGetValue("ReturnType", out object? returnValue) ? returnValue.ToString() : "void";
            var parameters = node.TryGetValue("Parameters", out object? paramValue) ? FormatParameters(paramValue) : "";

            luauCode.AppendLine($"{indent}function {methodName}({parameters})");
            if (returnType != "void")
            {
                if (returnType != null)
                {
                    luauCode.AppendLine($"{indent}  return {FormatReturnType(returnType)}");
                }
            }

            ProcessChildren(node, luauCode, indentLevel + 1);
            luauCode.AppendLine($"{indent}end");
        }

        /// <summary>
        /// Processes the child nodes of a given AST node and appends their Luau code representation.
        /// </summary>
        private static void ProcessChildren(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            if (node.ContainsKey("Children"))
            {
                var children = (List<Dictionary<string, object>>)node["Children"];
                foreach (var childNode in children)
                {
                    ProcessNode(childNode, luauCode, indentLevel);
                }
            }
        }

        /// <summary>
        /// Formats parameters for a Luau function.
        /// </summary>
        private static string FormatParameters(object parameters)
        {
            var parameterList = (List<dynamic>)parameters;
            return string.Join(", ", parameterList.Select(p => p.Name));
        }

        /// <summary>
        /// Utility function to format a return type for a method.
        /// </summary>
        private static string FormatReturnType(string returnType)
        {
            return returnType switch
            {
                "void" => "",
                "int" => "number",
                "string" => "string",
                "bool" => "boolean",
                _ => returnType // For custom types or unrecognized types
            };
        }
    }
}
