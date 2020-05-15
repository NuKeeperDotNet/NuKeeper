using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Commands;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NuKeeper.Tests")]

#pragma warning disable CA1822
#pragma warning disable CA1031

namespace NuKeeper
{
    [Command(
        Name = "NuKeeper",
        FullName = "Automagically update NuGet packages in .NET projects.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [Subcommand(typeof(InspectCommand))]
    [Subcommand(typeof(UpdateCommand))]
    [Subcommand(typeof(RepositoryCommand))]
    [Subcommand(typeof(OrganisationCommand))]
    [Subcommand(typeof(GlobalCommand))]
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var container = ContainerRegistration.Init();

            var app = new CommandLineApplication<Program> { UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect };
            app.Conventions.UseDefaultConventions().UseConstructorInjection(container);

            try
            {
                return await app.ExecuteAsync(args);
            }
            catch (CommandParsingException cpe)
            {
                Console.WriteLine(cpe.Message);
                return (int)ExitCodes.InvalidArguments;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (int)ExitCodes.UnknownError;
            }
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
