using System.Runtime.InteropServices;

namespace NuKeeper.Inspection.Tests
{
    public static class OsSpecifics
    {
        public static string GenerateBaseDirectory()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var baseDirectory = isWindows ? "c:\\temp\\somewhere" : "/temp/somewhere";
            return baseDirectory;
        }
    }
}
