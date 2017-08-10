namespace NuKeeper.Files
{
    public  interface IFolder
    {
        string FullPath { get; }
        void TryDelete();
    }
}