using System;

namespace NuKeeper.Abstractions.DTOs
{
    public class ForkData
    {
        public ForkData(Uri uri, string owner, string name)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Uri = uri;
            Owner = owner;
            Name = name;
        }

        public Uri Uri { get; }

        public string Owner { get; }

        public string Name { get; }
    }
}
