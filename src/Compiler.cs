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

            File.WriteAllText(outputFilePath + ".luau", luaCode);
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

    public static class CompilerLSConverter
    {
        private static readonly Dictionary<string, Action<Dictionary<string, object>, StringBuilder, int>> NodeHandlers;

        static CompilerLSConverter()
        {
            NodeHandlers = new Dictionary<string, Action<Dictionary<string, object>, StringBuilder, int>>
        {
            { "Namespace", HandleNamespaceDeclaration },
            { "Class", HandleClassDeclaration },
            { "Method", HandleMethodDeclaration }
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
            luauCode.AppendLine($"{indent}local {namespaceName} = {{}}");  // Create a table for the namespace

            ProcessChildren(node, luauCode, indentLevel + 1); // Process nested classes or other elements
        }

        private static void HandleClassDeclaration(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var className = node.TryGetValue("Name", out var value) ? value?.ToString() : "UnnamedClass";
            var classNamespace = node.TryGetValue("Namespace", out var namespaceValue) ? namespaceValue?.ToString() : null;

            if (classNamespace != null)
            {
                // Add class to its parent namespace
                luauCode.AppendLine($"{indent}-- Class: {className}");
                luauCode.AppendLine($"{indent}{classNamespace}.{className} = {{}}");
            }
            else
            {
                // Root class
                luauCode.AppendLine($"{indent}-- Class: {className}");
                luauCode.AppendLine($"{indent}local {className} = {{}}");  // Root-level class as a table
            }

            ProcessChildren(node, luauCode, indentLevel + 1); // Process methods and properties inside the class
        }

        private static void HandleMethodDeclaration(Dictionary<string, object> node, StringBuilder luauCode, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            var methodName = node.TryGetValue("Name", out var value) ? value?.ToString() : "UnnamedMethod";
            var returnType = node.TryGetValue("ReturnType", out var returnValue) ? returnValue?.ToString() : "void";
            var parameters = node.TryGetValue("Parameters", out var paramValue) ? FormatParameters(paramValue) : "";

            // Method declaration in Luau
            luauCode.AppendLine($"{indent}function {methodName}({parameters})");

            // Process method body if available
            var methodBody = node.TryGetValue("Body", out var bodyValue) ? bodyValue?.ToString() : "";
            if (!string.IsNullOrEmpty(methodBody))
            {
                var bodyIndent = new string(' ', (indentLevel + 1) * 2);
                luauCode.AppendLine($"{bodyIndent}{methodBody}");
            }

            if (returnType != "void")
            {
                if (returnType != null)
                {
                    luauCode.AppendLine($"{indent}  return {FormatReturnType(returnType)}");
                }
            }

            luauCode.AppendLine($"{indent}end");
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
