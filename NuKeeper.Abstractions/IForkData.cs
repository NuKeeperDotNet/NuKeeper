using System;

namespace NuKeeper.Abstractions
{
    public interface IForkData
    {
        Uri Uri { get; }
        string Name { get; }
        string Owner { get; }
    }
}
