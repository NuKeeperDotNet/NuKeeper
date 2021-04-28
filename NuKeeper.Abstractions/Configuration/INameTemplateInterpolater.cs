using System.Collections.Generic;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Engine
{
    public interface INameTemplateInterpolater
    {
        /// <summary>
        /// Replaces the tokens in the name template with the predefined values. 
        /// </summary>
        /// <param name="updates"></param>
        /// <param name="nameTemplate"></param>
        /// <returns></returns>
        string MakeName(IReadOnlyCollection<PackageUpdateSet> updates, string nameTemplate = null);
    }
}
