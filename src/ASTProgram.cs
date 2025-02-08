// Imports //
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Namespace //
namespace LuaSharp.src
{
    /// <summary>
    /// Provides methods to fetch and analyze the Abstract Syntax Tree (AST) of C# source code.
    /// </summary>
    /// <remarks>
    /// This class includes methods to parse C# source code into a syntax tree, extract namespace declarations,
    /// class declarations within namespaces, class declarations in the root, and method declarations within classes.
    /// </remarks>
    public static class ASTFetcher
    {
        /// <summary>
        /// Returns a syntax tree root for a specific piece of .cs source code.
        /// </summary>
        /// <param name="sourceCode">The C# source code.</param>
        /// <returns>
        /// <c>SyntaxNode</c>
        /// </returns>
        public static SyntaxNode GetSyntaxTreeRoot(string sourceCode)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = syntaxTree.GetRoot();
            return root;
        }

        /// <summary>
        /// Extracts and prints all namespace declarations in the provided syntax tree root.
        /// </summary>
        /// <param name="syntaxTreeRoot">The syntax tree root.</param>
        public static IEnumerable<NamespaceDeclarationSyntax> GetNamespaceDeclarations(SyntaxNode syntaxTreeRoot)
        {
            // Use direct LINQ query without intermediate variable
            return syntaxTreeRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
        }

        /// <summary>
        /// Extracts class declarations inside a specific namespace.
        /// </summary>
        /// <param name="namespaceNode">The namespace syntax node.</param>
        /// <returns>List of class declaration syntax nodes inside the namespace.</returns>
        public static List<ClassDeclarationSyntax> GetClassDeclarationsInsideNamespace(NamespaceDeclarationSyntax namespaceNode)
        {
            var classDeclarations = namespaceNode.DescendantNodes()
                .OfType<ClassDeclarationSyntax>();
            return [.. classDeclarations];
        }

        /// <summary>
        /// Finds all the classes in the root of a syntax tree.
        /// </summary>
        /// <param name="syntaxTreeRoot">The syntax tree root of a source code.</param>
        /// <returns>
        //  <c>List<ClassDeclerationSyntax></c>
        //  </returns>
        public static List<ClassDeclarationSyntax> GetClassesInRoot(SyntaxNode syntaxTreeRoot)
        {
            var classDeclarations = syntaxTreeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
            return [.. classDeclarations];
        }

        /// <summary>
        /// Extracts method declarations inside a specific class.
        /// </summary>
        /// <param name="classNode">The class syntax node.</param>
        /// <returns>List of method declaration syntax nodes inside the class.</returns>
        public static List<MethodDeclarationSyntax> GetMethodDeclarationsInsideClass(ClassDeclarationSyntax classNode)
        {
            var methodDeclarations = classNode.DescendantNodes()
                .OfType<MethodDeclarationSyntax>();
            return [.. methodDeclarations];
        }
    }

    /// <summary>
    /// Provides methods to parse and build the Abstract Syntax Tree (AST) of C# source code.
    /// </summary>
    /// <remarks>
    /// This class includes methods to parse C# source code into a syntax tree, extract a full AST tree,
    /// and ensure it builds properly to give an output suitable for Luau conversion.
    /// </remarks>
    public static class ASTParser
    {
        /// <summary>
        /// Recursively processes the syntax tree and generates a simplified AST structure with proper parenting and hierarchy.
        /// </summary>
        /// <param name="syntaxTreeRoot">The root node of the C# syntax tree.</param>
        /// <returns>A list representing the simplified AST with proper parenting and hierarchy.</returns>
        public static List<Dictionary<string, object>> GetFullAST(SyntaxNode syntaxTreeRoot)
        {
            var ast = new List<Dictionary<string, object>>();

            // Fetch all namespaces
            var namespaces = ASTFetcher.GetNamespaceDeclarations(syntaxTreeRoot);
            foreach (var namespaceNode in namespaces)
            {
                var namespaceRepresentation = ProcessNamespaceNode(namespaceNode);

                // Fetch classes inside the namespace
                var classesInNamespace = ASTFetcher.GetClassDeclarationsInsideNamespace(namespaceNode);
                foreach (var classNode in classesInNamespace)
                {
                    var classRepresentation = ProcessClassNode(classNode);
                    ((List<Dictionary<string, object>>)namespaceRepresentation["Classes"]).Add(classRepresentation);
                }

                ast.Add(namespaceRepresentation); // Add the namespace and its classes
            }

            // Handle classes outside of namespaces (root level classes)
            var rootClasses = ASTFetcher.GetClassesInRoot(syntaxTreeRoot);
            foreach (var classNode in rootClasses)
            {
                var classRepresentation = ProcessClassNode(classNode);
                ast.Add(classRepresentation); // Add root classes
            }

            return ast;
        }

        /// <summary>
        /// Processes a namespace node and generates its representation.
        /// </summary>
        /// <param name="namespaceNode">The namespace syntax node.</param>
        /// <returns>A dictionary representing the namespace and its contents.</returns>
        private static Dictionary<string, object> ProcessNamespaceNode(NamespaceDeclarationSyntax namespaceNode)
        {
            var namespaceRepresentation = new Dictionary<string, object>
        {
            { "NodeType", "Namespace" },
            { "Name", namespaceNode.Name.ToString() }
        };

            // Fetch classes using ASTFetcher
            var classes = ASTFetcher.GetClassDeclarationsInsideNamespace(namespaceNode);
            namespaceRepresentation["Classes"] = classes.Select(ProcessClassNode).ToList();

            return namespaceRepresentation;
        }

        /// <summary>
        /// Processes a class node and generates its representation.
        /// </summary>
        /// <param name="classNode">The class syntax node.</param>
        /// <returns>A dictionary representing the class and its contents.</returns>
        private static Dictionary<string, object> ProcessClassNode(ClassDeclarationSyntax classNode)
        {
            var classRepresentation = new Dictionary<string, object>
        {
            { "NodeType", "Class" },
            { "Name", classNode.Identifier.Text }
        };

            // Fetch methods using ASTFetcher
            var methods = ASTFetcher.GetMethodDeclarationsInsideClass(classNode);
            classRepresentation["Methods"] = methods.Select(ProcessMethodNode).ToList();

            return classRepresentation;
        }

        /// <summary>
        /// Processes a method node and generates its representation.
        /// </summary>
        /// <param name="methodNode">The method syntax node.</param>
        /// <returns>A dictionary representing the method.</returns>
        private static Dictionary<string, object> ProcessMethodNode(MethodDeclarationSyntax methodNode)
        {
            var methodRepresentation = new Dictionary<string, object>
        {
            { "NodeType", "Method" },
            { "Name", methodNode.Identifier.Text },
            { "ReturnType", methodNode.ReturnType.ToString() },
            { "Parameters", methodNode.ParameterList.Parameters
                .Select(p => new
                {
                    Name = p.Identifier.Text,
                    Type = p.Type?.ToString()
                }).ToList()
            }
        };

            return methodRepresentation;
        }
    }

    /// <summary>
    /// Just, utility or debug functions for the AST program.
    /// </summary>
    public static class ASTUtility
    {
        /// <summary>
        /// Recursively prints the AST in a tree-like structure based on the simplified list of dictionaries.
        /// </summary>
        /// <param name="ast">The list of dictionaries representing the simplified AST structure.</param>
        /// <param name="indentLevel">The level of indentation for formatting (used for recursive calls).</param>
        /// <param name="parentNodeName">The name of the parent node, used for clarity in the tree structure (optional).</param>
        public static void PrintAstTree(List<Dictionary<string, object>> ast, int indentLevel = 0, string? parentNodeName = null)
        {
            // Iterate over each node in the AST list and print it in a tree structure
            foreach (var node in ast)
            {
                PrintNode(node, indentLevel, parentNodeName ?? string.Empty);
            }
        }

        /// <summary>
        /// Prints an individual node from the AST, including its type, name, and properties, in a readable format.
        /// </summary>
        /// <param name="node">The node represented as a dictionary.</param>
        /// <param name="indentLevel">The level of indentation for formatting.</param>
        /// <param name="parentNodeName">The name of the parent node (optional).</param>
        private static void PrintNode(Dictionary<string, object> node, int indentLevel, string parentNodeName = "")
        {
            // Indentation for hierarchical tree structure
            var indent = new string(' ', indentLevel * 2);

            // Get the node type and kind (basic properties from the dictionary)
            var nodeType = node.ContainsKey("NodeType") ? node["NodeType"].ToString() : "Unknown";
            var kind = node.ContainsKey("Kind") ? node["Kind"].ToString() : "Unknown";

            // Start constructing the node's description
            var nodeDescription = $"{indent}{nodeType} - Kind: {kind}";

            // Add specific properties for different node types (Namespace, Class, Method, etc.)
            if (nodeType == "NamespaceDeclarationSyntax" && node.ContainsKey("Name"))
            {
                nodeDescription += $" - Namespace: {node["Name"]}";
            }
            else if (nodeType == "ClassDeclarationSyntax" && node.ContainsKey("Name"))
            {
                nodeDescription += $" - Class: {node["Name"]}";
            }
            else if (nodeType == "MethodDeclarationSyntax" && node.ContainsKey("Name"))
            {
                nodeDescription += $" - Method: {node["Name"]}, ReturnType: {node["ReturnType"]}";
                if (node.ContainsKey("Parameters"))
                {
                    nodeDescription += ", Parameters: " + string.Join(", ", ((List<dynamic>)node["Parameters"]).Select(p => $"{p.Name}: {p.Type}"));
                }
            }

            // Print the node's description
            if (parentNodeName != null)
            {
                Console.WriteLine($"{indent}Parent: {parentNodeName} -> {nodeDescription}");
            }
            else
            {
                Console.WriteLine(nodeDescription);
            }

            // Print child nodes (recursively if they exist)
            if (node.ContainsKey("Children"))
            {
                var childNodes = (List<Dictionary<string, object>>)node["Children"];
                foreach (var childNode in childNodes)
                {
                    PrintNode(childNode, indentLevel + 1, nodeDescription);
                }
            }
        }
    }
}
