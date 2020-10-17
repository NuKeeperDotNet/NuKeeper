namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ITemplateRenderer
    {
#pragma warning disable CA1716 // Identifiers should not match keywords
        string Render(string template, object view);
#pragma warning restore CA1716 // Identifiers should not match keywords
    }
}
