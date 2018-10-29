namespace NuKeeper.Abstractions.Engine
{
    public interface IRepositoryData
    {
        ForkData Pull { get; }
        ForkData Push { get; }
        string DefaultBranch { get; set; }
    }
}
