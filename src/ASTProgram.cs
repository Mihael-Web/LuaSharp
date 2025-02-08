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
            var namespaceDeclarations = syntaxTreeRoot.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>();
            return namespaceDeclarations;
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
        /// Recursively processes the syntax tree and generates a simplified AST structure.
        /// </summary>
        /// <param name="syntaxTreeRoot">The root node of the C# syntax tree.</param>
        /// <returns>A list representing the simplified AST.</returns>
        public static List<Dictionary<string, object>> GetFullAST(SyntaxNode syntaxTreeRoot)
        {
            var ast = new List<Dictionary<string, object>>();

            // Fetch namespaces using ASTFetcher //
            var namespaces = ASTFetcher.GetNamespaceDeclarations(syntaxTreeRoot);
            foreach (var namespaceNode in namespaces)
            {
                ast.Add(ProcessNamespaceNode(namespaceNode));
            }

            // Handle classes in the root (outside of namespaces) //
            var rootClasses = ASTFetcher.GetClassesInRoot(syntaxTreeRoot);
            foreach (var classNode in rootClasses)
            {
                ast.Add(ProcessClassNode(classNode));
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
}
