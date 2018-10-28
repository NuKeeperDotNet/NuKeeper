namespace NuKeeper.Abstractions.Engine
{
    public interface IRepositoryData
    {
        IForkData Pull { get; }
        IForkData Push { get; }
        string DefaultBranch { get; set; }
    }
}
