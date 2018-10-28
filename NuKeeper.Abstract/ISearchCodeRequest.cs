using System.Collections.Generic;

namespace NuKeeper.Abstract
{
    public interface ISearchCodeRequest
    {
        string Term { get; }
        IList<KeyValuePair<string,string>> Repos { get; }
        int PerPage { get; }
        IList<CodeInQualifier> SearchIn { get; }
    }
}
