using System;

namespace NuKeeper.Abstractions
{
    public class PackagePath
    {
        public PackagePath(string relativePath, string fullName)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException("relative path is required", nameof(relativePath));
            }

            RelativePath = relativePath;
            FullName = fullName;
        }

        /// <summary>
        /// Path from BaseDirectory to the file, includes file name
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Full path to the file, 
        /// directory and file name
        /// </summary>
        public string FullName { get; }
    }
}
