// Imports //
using System.Text.Json;

// Namespace //
namespace LuaSharp.src.Utilities
{
    public static class ParseUtilLS
    {
        /// <summary>
        /// Parses the project json file and returns dictionary of configurations.
        /// </summary>
        /// <param name="file">The file to parse through.</param>
        /// <returns>
        //  <c>Dictionary</c> of project configurations after JSON has been read.
        //  </returns>
        public static Dictionary<string, object>? ParseProjectJsonFile(string file)
        {
            try
            {
                string jsonString = File.ReadAllText(file);
                var projectSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

                if (projectSettings != null)
                {
                    return projectSettings;
                }
                else
                {
                    Console.WriteLine("[LuaShrp] Failed to deserialize project settings.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[LuaShrp] An error occurred while reading the project settings file: " + ex.Message);
            }

            return null;
        }
    }
}
