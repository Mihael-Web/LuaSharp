using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LuaSharp.src
{
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
        /// Extracts method declarations inside a specific class.
        /// </summary>
        /// <param name="classNode">The class syntax node.</param>
        /// <returns>List of method names inside the class.</returns>
        public static List<string> GetMethodDeclarationsInsideClass(ClassDeclarationSyntax classNode)
        {
            var methodNames = new List<string>();
            var methodDeclarations = classNode.DescendantNodes()
                .OfType<MethodDeclarationSyntax>();

            foreach (var methodDeclaration in methodDeclarations)
            {
                methodNames.Add(methodDeclaration.Identifier.Text);
            }

            return methodNames;
        }
    }
}
