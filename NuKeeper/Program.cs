using System;
using NuKeeper.Nuget.Api;

namespace NuKeeper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Supply a git url");
                return;
            }
            var gitArg = args[0];

            var gitUri = new Uri(gitArg);

            var engine = new Engine(new PackageUpdatesLookup(new ApiPackageLookup()));
            engine.Run(gitUri)
                .GetAwaiter().GetResult();
        }
    }
}
