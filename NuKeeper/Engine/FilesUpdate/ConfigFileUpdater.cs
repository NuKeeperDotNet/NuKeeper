using NuGet.Packaging.Core;
using System.Text.RegularExpressions;

namespace NuKeeper.Engine.FilesUpdate
{
    public class ConfigFileUpdater
    {
        private const string RegexpTemplate =
@"<dependentAssembly>\s*<assemblyIdentity name=""{PackageId}"" publicKeyToken=""\w*""[\w\s"" =]*\/>\s*<bindingRedirect oldVersion=""[\d|\.]+-{targetVersion}"" newVersion=""{targetVersion}"" \/>\s*<\/dependentAssembly>";

        private readonly PackageIdentity _from;
        private readonly PackageIdentity _to;
        private readonly Regex _matcher;

        public ConfigFileUpdater(PackageIdentity from, PackageIdentity to)
        {
            _from = from;
            _to = to;
            _matcher = BuildMatcher();
        }

        private Regex BuildMatcher()
        {
            var packageTemplate = RegexpTemplate
                .Replace("{PackageId}", EscapeDots(_from.Id))
                .Replace("{targetVersion}", EscapeDots(_from.Version.ToString()));

            return new Regex(packageTemplate, RegexOptions.Compiled | RegexOptions.Multiline);
        }

        private static string EscapeDots(string value)
        {
            return value.Replace(".", "\\.");
        }

        public string ApplyUpdate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            return _matcher.Replace(input, Eval);
        }

        private string Eval(Match match)
        {
            var matchText = match.Groups[0].Value;
           return matchText.Replace(_from.Version.ToString(), _to.Version.ToString());
        }
    }
}
