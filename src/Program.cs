// Imports //
using LuaSharp.src.Utilities;
using LuaSharp.src.SessionManager;
// Program Class //
class LuaSharpProgram
{
    static void Main(string[] args)
    {
        // Confirm it's path //
        bool isRunningFromPath = PathUtilLS.IsRunningFromPath();
        if (!isRunningFromPath)
        {
            Console.WriteLine("[LuaShrp] Cannot run program due to it not being run from PATH.");
            return;
        }

        // Validate if directory or such is specified //
        bool hasDirSpecified = PathUtilLS.HasDirectorySpecified(args);

        // Complete validation before starting session //
        if (!hasDirSpecified)
        {
            Console.WriteLine("[LuaShrp] Unexpected error. Directory isn't specified while being ran in PATH.");
            return;
        }

        // Start session in the specified directory
        string directory = args[0];
        bool? watchMode = null;
        if (args.Length > 1 && args[1] == "--watch")
        {
            watchMode = true;
        }

        SessionManager.StartSessionInDirectory(directory, watchMode);
    }
}
