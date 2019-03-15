using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.EndToEndTests
{
    public class TestLogger : INuKeeperLogger
    {
        private readonly List<string> _messages = new List<string>();

        public void Error(string message, Exception ex = null)
        {
            _messages.Add(message);
        }

        public void Minimal(string message)
        {
            _messages.Add(message);
        }

        public void Normal(string message)
        {
            _messages.Add(message);
        }

        public void Detailed(string message)
        {
            _messages.Add(message);
        }

        public IReadOnlyCollection<string> Messages => _messages;

        public bool ReceivedMessage(string match)
        {
            return Messages.Any(m => m.Equals(match, StringComparison.Ordinal));
        }
    }
}
