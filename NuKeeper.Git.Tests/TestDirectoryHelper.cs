using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace NuKeeper.Git.Tests
{
    public static class TestDirectoryHelper
    {
        private static readonly Type[] whitelist = { typeof(IOException), typeof(UnauthorizedAccessException) };

        public static string DiscoverPathToGit()
        {
            var env = Environment.GetEnvironmentVariable("PATH")
                .Split(System.IO.Path.PathSeparator);

            foreach (var path in env)
            {
                try
                {
                    var files = Directory.GetFiles(path).Where(x => Path.GetFileNameWithoutExtension(x) == "git");
                    if (files.Any())
                    {
                        return files.FirstOrDefault();
                    }
                }
                catch(DirectoryNotFoundException)
                {
                }
            }

            return null;
        }

        public static DirectoryInfo UniqueTemporaryFolder()
        {
            var uniqueName = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var folder = Path.Combine(Path.GetTempPath(), "NuKeeper", uniqueName);

            var tempDir = new DirectoryInfo(folder);
            tempDir.Create();

            return tempDir;
        }

        public static void DeleteDirectory(DirectoryInfo toDelete)
        {
            if (toDelete == null)
            {
                throw new ArgumentNullException(nameof(toDelete));
            }

            // http://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true/329502#329502
            if (!toDelete.Exists)
            {
                Console.WriteLine($"Directory '{toDelete.FullName}' is missing and can't be removed.");
                return;
            }

            NormalizeAttributes(toDelete);
            DeleteDirectory(toDelete, maxAttempts: 5, initialTimeout: 16, timeoutFactor: 2);
        }

        private static void NormalizeAttributes(DirectoryInfo toNormalize)
        {
            FileInfo[] files = toNormalize.GetFiles();
            DirectoryInfo[] subdirectories = toNormalize.GetDirectories();

            foreach (var file in files)
            {
                File.SetAttributes(file.FullName, FileAttributes.Normal);
            }

            foreach (var subdirectory in subdirectories)
            {
                NormalizeAttributes(subdirectory);
            }

            File.SetAttributes(toNormalize.FullName, FileAttributes.Normal);
        }

        private static void DeleteDirectory(DirectoryInfo toDelete, int maxAttempts, int initialTimeout, int timeoutFactor)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    toDelete.Delete(true);
                    return;
                }
                catch (Exception ex)
                {
                    var caughtExceptionType = ex.GetType();

                    if (!whitelist.Any(knownExceptionType => knownExceptionType.IsAssignableFrom(caughtExceptionType)))
                    {
                        throw;
                    }

                    if (attempt < maxAttempts)
                    {
                        Thread.Sleep(initialTimeout * (int)Math.Pow(timeoutFactor, attempt - 1));
                        continue;
                    }

                    Console.WriteLine("{0}The directory '{1}' could not be deleted ({2} attempts were made) due to a {3}: {4}" +
                                                  "{0}Most of the time, this is due to an external process accessing the files in the temporary repositories created during the test runs, and keeping a handle on the directory, thus preventing the deletion of those files." +
                                                  "{0}Known and common causes include:" +
                                                  "{0}- Windows Search Indexer (go to the Indexing Options, in the Windows Control Panel, and exclude the bin folder of LibGit2Sharp.Tests)" +
                                                  "{0}- Antivirus (exclude the bin folder of LibGit2Sharp.Tests from the paths scanned by your real-time antivirus)" +
                                                  "{0}- TortoiseGit (change the 'Icon Overlays' settings, e.g., adding the bin folder of LibGit2Sharp.Tests to 'Exclude paths' and appending an '*' to exclude all subfolders as well)",
                        Environment.NewLine, toDelete.FullName, maxAttempts, caughtExceptionType, ex.Message);
                }
            }
        }
    }
}
