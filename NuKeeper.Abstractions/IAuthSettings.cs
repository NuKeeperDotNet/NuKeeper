using System;

namespace NuKeeper.Abstractions
{
    public interface IAuthSettings
    {
        Uri ApiBase { get; }
        string Token { get; }
    }
}
