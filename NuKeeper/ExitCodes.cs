using System;

namespace NuKeeper
{
    [Flags]
    enum ExitCodes
    {
        Success = 0,
        UnknownError = 1 << 0,
        InvalidArguments = 1 << 1,
        OtherError = 1 << 2,
    }
}
