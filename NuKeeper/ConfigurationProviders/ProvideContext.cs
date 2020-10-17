#pragma warning disable CA2208 // Instantiate argument exceptions correctly
#pragma warning disable CA1307 // Specify StringComparison
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuKeeper.ConfigurationProviders
{
    internal class ProvideContext : IProvideConfiguration
    {
        private readonly IFileSettingsCache _fileSettings;
        private readonly CommandBase _command;

        public ProvideContext(IFileSettingsCache fileSettings, CommandBase command)
        {
            _fileSettings = fileSettings;
            _command = command;
        }

        public async Task<ValidationResult> ProvideAsync(SettingsContainer settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            try
            {
                var commitTemplateContext = _command.Context?.ToDictionary(
                    kvp => kvp.Substring(0, kvp.IndexOf('=', 0)),
                    kvp => (object)kvp.Substring(kvp.IndexOf('=', 0) + 1)
                ) ?? _fileSettings.GetSettings().Context;

                foreach (var property in commitTemplateContext?.Keys ?? Enumerable.Empty<string>())
                {
                    settings.UserSettings.Context.Add(property, commitTemplateContext[property]);
                }

                return await ParseDelegatesAsync(settings.UserSettings.Context);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return ValidationResult.Failure(ex.Message);
            }
        }

        private static async Task<ValidationResult> ParseDelegatesAsync(IDictionary<string, object> context)
        {
            if (context == null) return ValidationResult.Success;

            const string delegateKey = "_delegates";

            if (context.ContainsKey(delegateKey))
            {
                var delegates = context[delegateKey] as IDictionary<string, string>
                    ?? JsonConvert.DeserializeObject<IDictionary<string, string>>(
                        context[delegateKey].ToString()
                    );

                foreach (var property in delegates.Keys)
                {
                    try
                    {
                        var @delegate = await ParseDelegateAsync(delegates[property]);
                        context.Add(property, @delegate);
                    }
                    catch (CompilationErrorException ex)
                    {
                        return ValidationResult.Failure(ex.Message);
                    }
                }

                context.Remove(delegateKey);
            }

            return ValidationResult.Success;
        }

        private static Task<object> ParseDelegateAsync(string delegateString)
        {
            return CSharpScript.EvaluateAsync(delegateString);
        }
    }
}
