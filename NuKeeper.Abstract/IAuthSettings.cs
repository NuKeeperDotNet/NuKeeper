using System;

namespace NuKeeper.Abstract
{
    public interface IAuthSettings
    {
        Uri ApiBase { get; }
        string Token { get; }
    }
}
