using System;

namespace NuKeeper.Abstract
{
    public interface IForkData
    {
        Uri Uri { get; set; }
        string Name { get; set; }
        string Owner { get; set; }
    }
}
