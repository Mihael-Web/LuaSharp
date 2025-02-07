namespace LuaSharp.src.Utilities
{
    public class TokenTreeHandler
    {
        public static void PrintTokenTree(Dictionary<string, object> tokenTree, int indentLevel = 0)
        {
            foreach (var kvp in tokenTree)
            {
                // Print the category key
                PrintIndentedLine($"{kvp.Key}:", indentLevel);

                if (kvp.Value is List<object> list)
                {
                    foreach (var item in list)
                    {
                        if (item is Dictionary<string, object> nestedDict)
                        {
                            // Handle nested structures for classes and methods
                            PrintClassOrMethod(nestedDict, indentLevel + 2);
                        }
                        else
                        {
                            // Simple value
                            PrintIndentedLine($"- {item}", indentLevel + 2);
                        }
                    }
                }
                else
                {
                    PrintIndentedLine($"- {kvp.Value}", indentLevel + 2);
                }
            }
        }

        private static void PrintClassOrMethod(Dictionary<string, object> details, int indentLevel)
        {
            // Print key-value pairs in the dictionary
            foreach (var detail in details)
            {
                if (detail.Value is List<object> nestedList)
                {
                    PrintIndentedLine($"{detail.Key}:", indentLevel);
                    foreach (var item in nestedList)
                    {
                        if (item is Dictionary<string, object> nestedDict)
                        {
                            // Recursively print nested structures
                            PrintClassOrMethod(nestedDict, indentLevel + 2);
                        }
                        else
                        {
                            PrintIndentedLine($"- {item}", indentLevel + 2);
                        }
                    }
                }
                else
                {
                    PrintIndentedLine($"{detail.Key}: {detail.Value}", indentLevel);
                }
            }
        }

        private static void PrintIndentedLine(string text, int indentLevel)
        {
            Console.WriteLine(new string(' ', indentLevel) + text);
        }
    }
}