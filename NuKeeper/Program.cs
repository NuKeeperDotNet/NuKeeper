using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Local;

namespace NuKeeper
{
    [Subcommand("inspect", typeof(LocalNuKeeperCommand))]
    [Subcommand("repository", typeof(GithubNuKeeperCommand))]
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

        protected int OnExecute(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return 1;
        }
    }

    internal class GithubNuKeeperCommand : CommandBase
    {
        private readonly GithubEngine _engine;

        public GithubNuKeeperCommand(GithubEngine engine)
        {
            _engine = engine;
        }

        public async Task<int> OnExecute(CommandLineApplication app, IConsole console)
        {
            await _engine.Run();
            return 0;
        }
    }

    internal class LocalNuKeeperCommand : CommandBase
    {
        private readonly SettingsContainer _settings;
        private readonly LocalEngine _engine;

        public LocalNuKeeperCommand(SettingsContainer settings, LocalEngine engine)
        {
            _settings = settings;
            _engine = engine;
        }

        public async Task<int> OnExecute(CommandLineApplication app, IConsole console)
        {
            await _engine.Run(_settings);
            return 0;
        }
    }

    [HelpOption]
    internal abstract class CommandBase
    {
    }
}
