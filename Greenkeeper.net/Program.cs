using System;
using System.Threading.Tasks;
using NuKeeper.ProcessRunner;

namespace NuKeeper
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            IExternalProcess proc = new ExternalProcess();
            var output = await proc.Run("dir");

            Console.WriteLine($"Exit code: {output.ExitCode}");
            Console.WriteLine(output.Output);
        }
    }
}
