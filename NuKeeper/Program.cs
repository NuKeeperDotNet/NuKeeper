using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Local;

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

            switch (settings.ModalSettings.Mode)
            {
                case RunMode.Inspect:
                case RunMode.Update:
                    var inpector = container.GetInstance<LocalEngine>();
                    await inpector.Run(settings);
                    break;

                case RunMode.Repository:
                case RunMode.Organisation:
                    var engine = container.GetInstance<GithubEngine>();
                    await engine.Run();
                    break;

                default:
                    throw new Exception($"Run mode '{settings.ModalSettings.Mode}' was not handled.");
            }

            return 0;
        }
    }
}
