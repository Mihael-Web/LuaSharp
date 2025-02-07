// Imports //
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Namespace //
namespace LuaSharp.src
{
    public static class MimicLexer
    {
        public static CompilationUnitSyntax GetCompilationUnitRootForSource(string source)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            return syntaxTree.GetCompilationUnitRoot();
        }

        public static Dictionary<string, object> TurnToDynamicTokenTree(CompilationUnitSyntax unitSyntaxRoot)
        {
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0028 // Simplify collection initialization
            Dictionary<string, object> tokenTree = new Dictionary<string, object>();
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0090 // Use 'new(...)'

            // Track the parent classes
            Stack<string> classStack = new Stack<string>();
            classStack.Push("Root"); // Default parent for top-level classes

            foreach (var node in unitSyntaxRoot.DescendantNodes())
            {
                string category;

                switch (node)
                {
                    case UsingDirectiveSyntax usingDirective:
                        category = "Usings";
                        if (usingDirective.Name != null)
                        {
                            AddToTokenTree(tokenTree, category, usingDirective.Name.ToString());
                        }
                        break;

                    case NamespaceDeclarationSyntax namespaceDeclaration:
                        category = "Namespaces";
                        AddToTokenTree(tokenTree, category, namespaceDeclaration.Name.ToString());
                        break;

                    case ClassDeclarationSyntax classDeclaration:
                        category = "Classes";
                        var parentClass = classStack.Peek();
                        var classDetails = GetClassDetails(classDeclaration, parentClass);

                        // Push the current class onto the stack
                        classStack.Push(classDeclaration.Identifier.Text);
                        AddToTokenTree(tokenTree, category, classDetails);
                        break;

                    case MethodDeclarationSyntax methodDeclaration:
                        category = "Methods";
                        var methodParentClass = classStack.Peek(); // Current parent class
                        var methodDetails = GetMethodDetails(methodDeclaration, methodParentClass);
                        AddToTokenTree(tokenTree, category, methodDetails);
                        break;

                    default:
                        break;
                }
            }

            return tokenTree;
        }

        private static Dictionary<string, object> GetClassDetails(ClassDeclarationSyntax classDeclaration, string parentClass)
        {
            var classDetails = new Dictionary<string, object>
            {
                { "ClassName", classDeclaration.Identifier.Text },
                { "ParentClass", parentClass },
                { "ClassSource", classDeclaration.ToFullString().Trim() }
            };

            return classDetails;
        }

        private static Dictionary<string, object> GetMethodDetails(MethodDeclarationSyntax methodDeclaration, string parentClass)
        {
            var methodDetails = new Dictionary<string, object>
            {
                { "Signature", $"{methodDeclaration.ReturnType} {methodDeclaration.Identifier.Text}" },
                { "Parameters", GetMethodParameters(methodDeclaration.ParameterList) },
                { "ParentClass", parentClass },
                { "MethodSource", methodDeclaration.ToFullString().Trim() }
            };

            return methodDetails;
        }

        private static List<string> GetMethodParameters(ParameterListSyntax parameterList)
        {
            var parameters = new List<string>();

            foreach (var parameter in parameterList.Parameters)
            {
                parameters.Add($"{parameter.Type} {parameter.Identifier.Text}");
            }

            return parameters;
        }

        private static void AddToTokenTree(Dictionary<string, object> tokenTree, string category, object value)
        {
            if (!tokenTree.ContainsKey(category))
            {
                tokenTree[category] = new List<object>();
            }
            ((List<object>)tokenTree[category]).Add(value);
        }
    }
}
