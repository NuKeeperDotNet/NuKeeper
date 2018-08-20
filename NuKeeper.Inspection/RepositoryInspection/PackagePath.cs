using System;
using System.IO;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class PackagePath
    {
        public PackagePath(string baseDirectory, string relativePath, 
            PackageReferenceType packageReferenceType)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                throw new ArgumentException(nameof(baseDirectory));
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentException(nameof(relativePath));
            }

            if (relativePath[0] == Path.DirectorySeparatorChar)
            {
                relativePath = relativePath.Substring(1);
            }

            BaseDirectory = baseDirectory;
            RelativePath = relativePath;
            PackageReferenceType = packageReferenceType;

            var fullPath = Path.Combine(baseDirectory, relativePath);
            Info = new FileInfo(fullPath);
        }

        public FileInfo Info { get;  }

        /// <summary>
        /// The working directory at the root of all the files
        /// </summary>
        public string BaseDirectory { get; }

        /// <summary>
        /// Path from BaseDirectory to the file, includes file name
        /// </summary>
        public string RelativePath { get; }

        public PackageReferenceType PackageReferenceType { get; }

        /// <summary>
        /// Full path to the file, 
        /// directory and file name
        /// </summary>
        public string FullName => Info.FullName;

        public override string ToString()
        {
            return $"{PackageReferenceType} {RelativePath} in {BaseDirectory}";
        }
    }
}
