using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.RepositoryInspection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Engine
{
    public static class BranchNamer
    {
        public static readonly string[] TemplateTokens = { "Default", "Name", "Version", "Count", "Hash" };

        private const string DefaultTemplateMultiple = "nukeeper-update-{Count}-packages-{Hash}";
        private const string DefaultTemplateSingle = "nukeeper-update-{Name}-to-{Version}";

        public static bool IsValidTemplateToken(string token)
        {
            return TemplateTokens.Any(t => t.Equals(token, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Replaces the tokens in the branchname with the predefined values. 
        /// </summary>
        /// <param name="updates"></param>
        /// <param name="branchTemplate"></param>
        /// <returns></returns>
        public static string MakeName(IReadOnlyCollection<PackageUpdateSet> updates, string branchTemplate = null)
        {
            var tokenValues = new Dictionary<string, string>();

            foreach (var token in TemplateTokens)
            {
                var value = "";
                switch (token)
                {
                    case "Default":
                        value = updates.Count == 1 ? DefaultTemplateSingle : DefaultTemplateMultiple;
                        break;
                    case "Name":
                        value = updates.Count > 1 ? "Multiple-Packages" : updates.First().SelectedId;
                        break;
                    case "Version":
                        //Multiple nugets, same version?
                        var versions = updates.Select(u => u.SelectedVersion).Distinct();
                        value = versions.Count() > 1 ? "Multiple-Versions" : $"{versions.First()}";
                        break;
                    case "Count":
                        value = $"{updates.Count}";
                        break;
                    case "Hash":
                        value = Hasher.Hash(PackageVersionStrings(updates));
                        break;
                }
                tokenValues.Add(token, value);
            }

            return MakeName(tokenValues, branchTemplate);
        }

        /// <summary>
        /// Replaces the tokens in the branchname with the given values
        /// </summary>
        /// <param name="tokenValuePairs"></param>
        /// <param name="branchTemplate"></param>
        /// <returns></returns>
        public static string MakeName(Dictionary<string, string> tokenValuePairs, string branchTemplate)
        {
            var branchName = branchTemplate ?? "{default}";

            foreach (KeyValuePair<string, string> kvp in tokenValuePairs)
            {
                branchName = branchName.Replace(string.Concat("{", kvp.Key, "}"), kvp.Value, StringComparison.InvariantCultureIgnoreCase);
            }
            return branchName.Replace(" ", "-", StringComparison.InvariantCultureIgnoreCase);
        }

        private static string PackageVersionStrings(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            return string.Join(",", updates.Select(PackageVersionString));
        }

        private static string PackageVersionString(PackageUpdateSet updateSet)
        {
            return $"{updateSet.SelectedId}-v{updateSet.SelectedVersion}";
        }
    }
}
