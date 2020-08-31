namespace NuKeeper.Abstractions.Configuration
{
    public interface IFileSettingsReader
    {
        FileSettings Read(string folder);
    }
}
