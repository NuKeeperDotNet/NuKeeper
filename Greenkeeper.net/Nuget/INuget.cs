using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Greenkeeper.Nuget
{
    interface INuget
    {
        Task<VersionComparisonResult> CompareVersions(NugetPackage package);

        Task UpdatePackage(NugetPackage package);
    }
}
