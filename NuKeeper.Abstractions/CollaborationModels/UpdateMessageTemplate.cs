using NuKeeper.Abstractions.CollaborationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public abstract class UpdateMessageTemplate
    {
        private IDictionary<string, object> _persistedContext { get; } = new Dictionary<string, object>();

        protected ITemplateRenderer Renderer { get; }

        protected UpdateMessageTemplate(ITemplateRenderer renderer)
        {
            Renderer = renderer;
            InitializeContext();
        }

        /// <summary>
        ///     Container for all placeholder replacement values, which will be passed to the <see cref="ITemplateRenderer.Render(string, object)"/> as the view.
        /// </summary>
        protected IDictionary<string, object> Context { get; } = new Dictionary<string, object>();

        /// <summary>
        ///     The template proper containing placeholders.
        /// </summary>
        public abstract string Value { get; }

        public IList<PackageTemplate> Packages
        {
            get
            {
                Context.TryGetValue(Constants.Template.Packages, out var packages);
                return packages as IList<PackageTemplate>;
            }
        }

        public FooterTemplate Footer
        {
            get
            {
                Context.TryGetValue(Constants.Template.Footer, out var footer);
                return footer as FooterTemplate;
            }
            set
            {
                Context[Constants.Template.Footer] = value;
            }
        }

        public bool MultipleChanges => Packages?.Count > 1;
        public int PackageCount => Packages?.Count ?? 0;
        public int ProjectsUpdated => Packages?
            .SelectMany(p => p.Updates)
            .Select(u => u.SourceFilePath)
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .Count() ?? 0;

        public bool MultipleProjects => ProjectsUpdated > 1;

        /// <summary>
        ///     Clear all current values for placeholders in the template.
        /// </summary>
        public virtual void Clear()
        {
            Context.Clear();
            InitializeContext();
        }

        /// <summary>
        ///     Add a new value for a placeholder to the template. This is only useful in case you define your own custom template.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="persist">Persist the value after <see cref="Clear"/></param>
        public void AddPlaceholderValue<T>(string key, T value, bool persist = false)
        {
            Context[key] = value;
            if (persist) _persistedContext[key] = value;
        }

        /// <summary>
        ///     Get the value from a placeholder in the template.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetPlaceholderValue<T>(string key)
        {
            if (Context.TryGetValue(key, out var value))
                return (T)value;
            else
                return default;
        }

        /// <summary>
        ///     Output the template with all its placeholders replaced by the current values.
        /// </summary>
        /// <returns></returns>
        public virtual string Output()
        {
            var packages = Packages
                .Select(
                    (p, i) => new
                    {
                        p.ActualChange,
                        p.AllowedChange,
                        p.LatestVersion,
                        p.Name,
                        p.ProjectsUpdated,
                        p.Publication,
                        Updates = p.Updates.Select(
                            (u, j) => new
                            {
                                u.SourceFilePath,
                                u.FromVersion,
                                u.FromUrl,
                                u.ToVersion,
                                Last = j == p.Updates.Length
                            }
                        ).ToArray(),
                        p.SourceUrl,
                        p.Url,
                        p.IsFromNuget,
                        p.Version,
                        p.FromVersion,
                        p.MultipleProjectsUpdated,
                        p.MultipleUpdates,
                        Last = i == Packages.Count
                    }
                )
                .ToArray();

            var context = new Dictionary<string, object>(Context)
            {
                [Constants.Template.MultipleChanges] = MultipleChanges,
                [Constants.Template.PackageCount] = PackageCount,
                [Constants.Template.Packages] = packages,
                [Constants.Template.ProjectsUpdated] = ProjectsUpdated,
                [Constants.Template.MultipleProjects] = MultipleProjects
            };

            return Renderer.Render(Value, context);
        }

        private void InitializeContext()
        {
            Context[Constants.Template.Packages] = new List<PackageTemplate>();
            foreach (var kvp in _persistedContext)
            {
                Context[kvp.Key] = kvp.Value;
            }
        }
    }
}
