namespace NuKeeper.Abstractions.Configuration
{
    public interface IEnvironmentVariablesProvider
    {
        string GetEnvironmentVariable(string name);
    }
}
