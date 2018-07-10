using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Commands;
using NuKeeper.Configuration;

namespace NuKeeper
{
    [Command(
        Name = "NuKeeper",
        FullName = "Automagically update NuGet packages in .NET projects.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [Subcommand("inspect", typeof(InspectCommand))]
    public class Program
    {
        public static int Main(string[] args)
        {
            var settings = SettingsParser.ReadSettings(args);

            if(settings == null)
            {
                Console.WriteLine("Exiting early...");
                return 1;
            }

            var container = ContainerRegistration.Init(settings);

            var app = new CommandLineApplication<Program> {ThrowOnUnexpectedArgument = false};
            app.Conventions.UseDefaultConventions().UseConstructorInjection(container);

            return app.Execute(args);
        }

        // ReSharper disable once UnusedMember.Global
        protected int OnExecute(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion() => typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;
    }
}
