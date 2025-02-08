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
            // Use Path.Combine instead of string concatenation for paths 
            // What if the user is on Linux? Or Mac? Or even a toaster?
            // Don't listen to the dude above, that dude's just a nerd.
            var outputFilePath = Path.Combine(outDir, Path.GetFileNameWithoutExtension(file) + ".luau");

            // Add null-coalescing for defensive programming (Square up, pal.)
            string sourceCode = File.ReadAllText(file) ?? string.Empty;

            if (string.IsNullOrEmpty(sourceCode))
            {
                Console.WriteLine($"[LuaShrp] [Compilation] Failed to get source code for file {file}");
                return;
            }

            string luaCode = CompilerLSConverter.ConvertCSharpToLuau(sourceCode);

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
        public static void AttemptBuild(string sourceDirectory, string outputDirectory, string mainDirectory)
        {
            // Use Path.Combine instead of string concatenation because fuck me 
            string outDirCombined = Path.Combine(mainDirectory, outputDirectory);

            // Ensure source directory is relative to main directory
            string sourceDirCombined = Path.Combine(mainDirectory, sourceDirectory);

            if (!Directory.Exists(outDirCombined))
            {
                Directory.CreateDirectory(outDirCombined);
            }

            // Use the combined source directory path
            foreach (var file in Directory.GetFiles(sourceDirCombined, "*.cs", SearchOption.AllDirectories))
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

    public static class CompilerLSConverter
    {
        private static readonly Dictionary<string, Action<Dictionary<string, object>, StringBuilder, int>> NodeHandlers;

        static CompilerLSConverter()
        {
            NodeHandlers = new Dictionary<string, Action<Dictionary<string, object>, StringBuilder, int>>
            {
                { "Namespace", HandleNamespaceDeclaration },
                { "Class", HandleClassDeclaration }
            };
        }

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

        private static void ProcessNode(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var nodeType = node.ContainsKey("NodeType") ? node["NodeType"]?.ToString() : "Unknown NodeType";

            if (nodeType != null && NodeHandlers.TryGetValue(nodeType, out var handler))
            {
                handler(node, luauCode, indentLevel);
            }
            else
            {
                ProcessGenericNode(node, luauCode, indentLevel);
            }
        }

        private static void ProcessGenericNode(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var nodeRepresentation = node.ContainsKey("NodeType") ? node["NodeType"].ToString() : "Unknown Node";
            luauCode.AppendLine($"{indent}-- {nodeRepresentation}");

            ProcessChildren(node, luauCode, indentLevel);
        }

        private static void HandleNamespaceDeclaration(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var namespaceName = node.TryGetValue("Name", out var value) ? value?.ToString() : "UnnamedNamespace";

            luauCode.AppendLine($"{indent}-- Namespace: {namespaceName}");
            luauCode.AppendLine($"{indent}local {namespaceName} = {{}}");
            luauCode.AppendLine($"{indent}{namespaceName}.__index = {namespaceName}\n");

            // Process classes within the namespace
            if (node.ContainsKey("Classes"))
            {
                if (node["Classes"] is List<Dictionary<string, object>> classes)
                {
                    foreach (var classNode in classes)
                    {
                        ProcessNode(classNode, luauCode, indentLevel + 1);
                    }
                }
            }

            ProcessChildren(node, luauCode, indentLevel);
        }

        private static void HandleClassDeclaration(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var className = node.TryGetValue("Name", out var value) ? value?.ToString() : "UnnamedClass";
            var parentNamespace = node.TryGetValue("Namespace", out var nsValue) ? nsValue?.ToString() : null;

            // Always declare the class locally first
            luauCode.AppendLine($"{indent}-- Class: {className}");
            luauCode.AppendLine($"{indent}local {className} = {{}}");
            luauCode.AppendLine($"{indent}{className}.__index = {className}\n");

            // Process methods first
            if (node.ContainsKey("Methods") && className != null)
            {
                var methods = node["Methods"] as List<Dictionary<string, object>>;
                if (methods != null)
                {
                    foreach (var method in methods)
                    {
                        HandleMethodDeclaration(method, luauCode, indentLevel, className);
                    }
                }
            }

            // After processing methods, assign the class to its namespace
            if (parentNamespace != null && parentNamespace != string.Empty)
            {
                luauCode.AppendLine($"\n{indent}{parentNamespace}.{className} = {className}");
            }

            luauCode.AppendLine(); // Add extra newline for readability
            // You know, this kinda fucked up the entire export system. Looks like shit.
            // Spaces and shit all over the place.
        }

        private static void HandleMethodDeclaration(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel, string className)
        {
            var indent = new string(' ', indentLevel * 2);
            var methodName = node.TryGetValue("Name", out var value) ? value?.ToString() : "UnnamedMethod";

            luauCode.AppendLine($"{indent}{className}.{methodName} = function(self)");
            luauCode.AppendLine($"{indent}    -- Method body here");
            luauCode.AppendLine($"{indent}end\n");
        }

        private static void ProcessChildren(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            if (node.TryGetValue("Children", out var value))
            {
                var children = value as List<Dictionary<string, object>>;
                if (children != null)
                {
                    foreach (var childNode in children)
                    {
                        ProcessNode(childNode, luauCode, indentLevel);
                    }
                }
            }
        }

        private static string FormatParameters(object parameters)
        {
            var parameterList = parameters as List<dynamic>;
            return parameterList != null ? string.Join(", ", parameterList.Select(p => p.Name)) : "";
        }

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
