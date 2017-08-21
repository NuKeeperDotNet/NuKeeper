using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;

namespace NuKeeper
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var settings = SettingsParser.ReadSettings(args);

            if(settings == null)
            {
                Console.WriteLine("Exiting early...");
                return 1;
            }

            var container = ContainerRegistration.Init(settings);
            var engine = container.GetInstance<GithubEngine>();
            await engine.Run();

            return 0;
        }
    }
}
