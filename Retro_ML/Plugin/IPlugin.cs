using Retro_ML.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.Plugin
{
    public interface IPlugin
    {
        /// <summary>
        /// The unique name of this plugin.
        /// </summary>
        string PluginName { get; }
        /// <summary>
        /// The default path of this plugin's config, and where the configuration will be saved.
        /// </summary>
        string PluginConfigPath { get; }
        /// <summary>
        /// Returns a new instance of the <see cref="IPluginConfig"/> specific to this plugin
        /// </summary>
        /// <returns></returns>
        IPluginConfig GetPluginConfig();
    }
}
