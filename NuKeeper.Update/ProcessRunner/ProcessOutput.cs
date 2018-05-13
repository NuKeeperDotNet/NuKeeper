namespace NuKeeper.Update.ProcessRunner
{
    public class ProcessOutput
    {
        public ProcessOutput(string output, string errorOutput, int exitCode)
        {
            Output = output;
            ErrorOutput = errorOutput;
            ExitCode = exitCode;
        }

        public string Output { get; }
        public string ErrorOutput { get; }
        public int ExitCode { get; }

        public bool Success => ExitCode == 0;
    }
}
