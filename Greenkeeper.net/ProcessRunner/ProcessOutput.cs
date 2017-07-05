namespace NuKeeper.ProcessRunner
{
    public class ProcessOutput
    {
        public ProcessOutput(string output, int exitCode)
        {
            Output = output;
            ExitCode = exitCode;
        }

        public string Output { get; }
        public int ExitCode { get; }

        public bool Success => ExitCode == 0;
    }
}