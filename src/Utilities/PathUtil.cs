// Imports //
using System;
using System.IO;

// Namespace //
namespace LuaSharp.src.Utilities
{
    /// <summary>
    /// Utility class for path-related operations.
    /// </summary>
    public static class PathUtilLS
    {
        /// <summary>
        /// Returns a boolean depending on if the executable is being ran from a PATH variable or just generally.
        /// </summary>
        /// <returns>
        /// <c>Boolean (true, false)</c>. True if from path, false if not from path.
        /// </returns>
        public static bool IsRunningFromPath()
        {
            // First we have to get the execution path, which is kinda simple.
            string executingPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            string[] pathDirectories = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();

            // After we fetch the thingy, we check if we're runnin from path!
            foreach (string dir in pathDirectories)
            {
                if (string.Equals(dir.Trim(), executingPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a boolean if the PATH-ran executable has a path specified.
        /// </summary>
        /// <returns>
        //  Returns a boolean which decides if the PATH-ran executable has a <c>path</c> specified.
        //  </returns>
        public static bool HasDirectorySpecified(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("[LuaShrp] No directory specified. Recommended to use current directory (.)");
                return false;
            }

            string directory = args[0];
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"[LuaShrp] The directory '{directory}' does not exist.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds a file inside a specified directory.
        /// </summary>
        /// <param name="directory">The directory to search in.</param>
        /// <param name="fileName">The name of the file to search for.</param>
        /// <returns>
        /// The full path of the file if found, otherwise <c>null</c>.
        /// </returns>
        public static string? FindFileInDirectory(string directory, string fileName)
        {
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"[LuaShrp] The directory '{directory}' does not exist. Can't find file " + fileName);
                return null;
            }

            string[] files = Directory.GetFiles(directory, fileName, SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                return files[0];
            }

            Console.WriteLine($"[LuaShrp] The file '{fileName}' was not found in directory '{directory}'.");
            return null;
        }
    }
}
