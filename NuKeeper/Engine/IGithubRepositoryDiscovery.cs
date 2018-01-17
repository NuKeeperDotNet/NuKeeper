﻿using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Configuration;

namespace NuKeeper.Engine
{
    public interface IGithubRepositoryDiscovery
    {
        Task<IEnumerable<RepositorySettings>> GetRepositories();
    }
}