#pragma warning disable CA1716 // Identifiers should not match keywords
using NuKeeper.Abstractions.CollaborationModels;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface IEnrichContext<in TSource, in TTemplate>
        where TTemplate : UpdateMessageTemplate
    {
        void Enrich(TSource source, TTemplate template);
    }
}
