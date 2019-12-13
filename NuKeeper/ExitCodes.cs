using System;

namespace NuKeeper
{
    [Flags]
    enum ExitCodes
    {
        Success = 0,
        UnknownError = 1 << 1,
        InvalidArguments = 1 << 2,
    }
}
