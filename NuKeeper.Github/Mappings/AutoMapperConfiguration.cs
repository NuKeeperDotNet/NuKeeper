using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using NuKeeper.Abstract;
using NuKeeper.Github.Engine;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public static class AutoMapperConfiguration
    {
        public static IMapper GithubMappingConfiguration { get; }

        [SuppressMessage("ReSharper", "CA1810")]
        static AutoMapperConfiguration()
        {
            var mapper = new Lazy<MapperConfiguration>(()=>
                new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<RepositoryPermissions, Permissions>();
                    cfg.CreateMap<Repository, GithubRepository>();
                    cfg.CreateMap<User, GithubAccount>();
                    cfg.CreateMap<User, GithubAccount>();
                    cfg.CreateMap<GithubPullRequest, NewPullRequest>();
                    cfg.CreateMap<GithubSearchCodeRequest, SearchCodeRequest>();
                    cfg.CreateMap<SearchCodeResult, GithubSearchCodeResult>();
                })
            );

            GithubMappingConfiguration = mapper.Value.CreateMapper();
        }
    }
}
