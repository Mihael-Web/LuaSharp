// Imports //
using LuaSharp.src;
using LuaSharp.src.Utilities;

// Namespace //
namespace LuaSharp.src.SessionManager
{
    public static class SessionManager
    {
        public static string? _cachedProjectSettingsFile;

        /// <summary>
        /// Starts a compilation session inside a specific directory.
        /// </summary>
        /// <param name="directory">The directory to traverse through and start session.</param>
        /// <param name="watch">If should be ran like watch mode or just build.</param>
        public static void StartSessionInDirectory(string directory, bool? watch)
        {
            // Validate project files //
            bool areValidProjectFiles = Validator.ValidateLuaSharpProjectFilesInDirectory(directory);

            // Display State //
            Console.WriteLine(areValidProjectFiles ?
                "[LuaShrp] Project files are valid. Configurations found." :
                "[LuaShrp] Project files not valid. Configurations not found.");

            // Accomodate to settings //
            string? projectSettingsFile = PathUtilLS.FindFileInDirectory(directory, "luasharp.settings.json");
            if (projectSettingsFile == null)
            {
                Console.WriteLine("[LuaShrp] Project file for some reason even after validation wasn't found inside directory " + directory);
                return;
            }

            _cachedProjectSettingsFile = projectSettingsFile;
            var parsedJson = ParseUtilLS.ParseProjectJsonFile(_cachedProjectSettingsFile);
            if (parsedJson == null)
            {
                Console.WriteLine("[LuaShrp] Failed to parse json in session. Closing session.");
                return;
            }

            // Display current mode //
            if (watch == true)
            {
                Console.WriteLine(@"
                ////
                // Running in WATCH mode. Each change will rebuild changed file.
                ////
                ");
            }
            else
            {
                Console.WriteLine(@"
                ////
                // Running in BUILD mode. Only will rebuild needed files and/or the whole project.
                ////
                ");
            }

            // Now, we are ready to do the compile stuff! //
            // Dunno why you're so happy. This is fucking bullshit. //
            string sourceDirectory = parsedJson["SourceDirectory"]?.ToString() ?? string.Empty;
            string outputDirectory = parsedJson["OutputDirectory"]?.ToString() ?? string.Empty;

            // Normalize the directory path
            directory = Path.GetFullPath(directory);

            Console.WriteLine($"[LuaShrp] Working Directory: {directory}");
            Console.WriteLine($"[LuaShrp] Source Directory: {sourceDirectory}");
            Console.WriteLine($"[LuaShrp] Output Directory: {outputDirectory}");

            if (sourceDirectory == string.Empty || outputDirectory == string.Empty)
            {
                Console.WriteLine("[LuaShrp] Failed to parse json configurations and apply them build. Check Source and Ouput directory.");
                return;
            }

            Console.WriteLine("[LuaShrp] Starting build session.");

            // Start compile //
            CompilerLS.AttemptBuild(sourceDirectory, outputDirectory, directory);
        }

        public static string? GetCachedProjectSettingsFile()
        {
            return _cachedProjectSettingsFile;
        }
    }
}
