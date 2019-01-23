using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.ProcessRunner;

namespace NuKeeper.Git
{
    public class GitCommandlinePath
    {
        private readonly AsyncLazy<string> _executablePath;

        private readonly INuKeeperLogger _logger;

        public GitCommandlinePath(INuKeeperLogger logger)
        {
            _logger = logger;
            _executablePath = new AsyncLazy<string>(FindExecutable);
        }

        public Task<string> Executable => _executablePath.Value;

        public async Task<bool> IsValid()
        {
            return await Executable != null;
        }

        private async Task<string> FindExecutable()
        {
            var process = new ExternalProcess(_logger);
            var result = await process.Run("", "git", "--version", null, false);

            return result.Success ? "git" : null;
        }
    }

    internal sealed class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(taskFactory)
        {
        }
    }
}
