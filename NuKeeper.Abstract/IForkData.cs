using System;

namespace NuKeeper.Abstract
{
    public interface IForkData
    {
        Uri Uri { get; }
        string Name { get; }
        string Owner { get; }
    }
}
