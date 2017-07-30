using System;
using System.IO;
using System.Threading.Tasks;
using NuKeeper.NuGet.Process;

namespace NuKeeper.Engine
{
    public class SolutionPrep
    {
        private readonly ISolutionRestore _slnRestore;

        public SolutionPrep(ISolutionRestore slnRestore)
        {
            _slnRestore = slnRestore;
        }

        public async Task Restore(string baseDir)
        {
            var solutions = Directory.GetFiles(baseDir, "*.sln", SearchOption.AllDirectories);

            foreach (var slnPath in solutions)
            {
                var path = Path.GetDirectoryName(slnPath);
                var file = Path.GetFileName(slnPath);
                await _slnRestore.Restore(path, file);
            }
        }
    }
}