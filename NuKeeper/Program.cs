using System.Reflection;
using System.Runtime.CompilerServices;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Commands;

[assembly:InternalsVisibleTo("NuKeeper.Tests")]

namespace NuKeeper
{
    [Command(
        Name = "NuKeeper",
        FullName = "Automagically update NuGet packages in .NET projects.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [Subcommand("inspect", typeof(InspectCommand))]
    [Subcommand("update", typeof(UpdateCommand))]
    [Subcommand("repository", typeof(RepositoryCommand))]
    [Subcommand("organisation", typeof(OrganisationCommand))]
    public class Program
    {
        public static int Main(string[] args)
        {
            var container = ContainerRegistration.Init();

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
