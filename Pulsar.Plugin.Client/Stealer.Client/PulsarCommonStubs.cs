using System;
using System.Collections.Generic;

namespace Pulsar.Common.Plugins
{
    public interface IUniversalPlugin
    {
        string PluginId { get; }
        string Version { get; }
        string[] SupportedCommands { get; }
        bool IsComplete { get; }
        void Initialize(object initData);
        PluginResult ExecuteCommand(string command, object parameters);
        void Cleanup();
    }

    public class PluginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool ShouldUnload { get; set; }
    }
}
