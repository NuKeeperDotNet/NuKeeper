using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace NuKeeper.Github
{
    public static class GithubResponseParser
    { 
        public static Uri GetNextUri(HttpResponseHeaders headers)
        {
            IEnumerable<string> linkCollection;
            var didGetLink = headers.TryGetValues("Link", out linkCollection);

            if (!didGetLink)
            {
                return null;
            }

            return GetNextUri(linkCollection.First());
        }

        public static Uri GetNextUri(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
            {
                throw new ArgumentNullException(nameof(link));
            }

            var regex = @".*<(.*)>.*next.*\z";

            var matches = Regex.Match(link, regex);

            if (matches.Groups.Count < 2)
            {
                return null;
            }

            Uri nextUri;
            Uri.TryCreate(matches.Groups[1].Value, UriKind.Absolute, out nextUri);
            return nextUri;
        }
    }
}