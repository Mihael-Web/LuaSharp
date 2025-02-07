// Imports //
// Namespace //
namespace LuaSharp.src.SessionManager
{
    public class Validator
    {
        /// <summary>
        /// Validates existance of important project files in a directory.
        /// </summary>
        /// <param name="directory">The directory to look for project files in.</param>
        /// <returns>
        //  <c>bool</c> which is a success state of the search.
        //  </returns>
        public static bool ValidateLuaSharpProjectFilesInDirectory(string directory)
        {
            // Display state //
            Console.WriteLine("[LuaShrp] Validating project files within main directory.");

            // Collect root info //
            string root = @directory;
            var directoryFiles = from file in Directory.EnumerateFiles(root) select file;
            bool hasFoundProjectsFile = false;

            // Enumerate //
            Console.WriteLine("[LuaShrp] Enumerating file count: {0}", directoryFiles.Count<string>().ToString());
            foreach (var file in directoryFiles)
            {
                if (Path.GetFileName(file) == "luasharp.settings.json" && Path.GetDirectoryName(file) == root)
                {
                    hasFoundProjectsFile = true;
                    break;
                }
            }

            // Return state //
            return hasFoundProjectsFile;
        }
    }
}
