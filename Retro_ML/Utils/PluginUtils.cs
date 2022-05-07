using Retro_ML.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.Utils
{
    internal static class PluginUtils
    {
        private static readonly Dictionary<string, IGamePlugin> gamePlugins = new();

        /// <summary>
        /// Returns a <see cref="IGamePlugin"/> instance from the plugins folder that's compatible with the given rom header name
        /// </summary>
        /// <param name="romHeaderName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IGamePlugin GetGamePlugin(string romHeaderName)
        {
            if (gamePlugins.ContainsKey(romHeaderName)) return gamePlugins[romHeaderName];

            var pluginType = typeof(IGamePlugin);
            foreach (var file in new DirectoryInfo(DefaultPaths.PLUGINS_DIR).GetFiles("*.dll"))
            {
                try
                {
                    Assembly a = AppDomain.CurrentDomain.Load(Assembly.LoadFrom(file.FullName).GetName());
                    var foundType = GetLoadableTypes(a)
                        .Where(pluginType.IsAssignableFrom)
                        .Where(t => t.IsClass)
                        .FirstOrDefault();
                    if (foundType != null)
                    {
                        var potentialPlugin = (IGamePlugin)Activator.CreateInstance(foundType)!;
                        if (potentialPlugin.PluginROMHeaderName == romHeaderName)
                        {
                            gamePlugins[potentialPlugin.PluginROMHeaderName] = potentialPlugin;
                            return potentialPlugin;
                        }
                    }
                }
                catch
                {
                    Exceptions.QueueException(new Exception($"Could not load assembly {file.Name}, is it a valid plugin?"));
                }
            }

            throw new Exception($"Could not find game plugin {romHeaderName} in {DefaultPaths.PLUGINS_DIR}");
        }

        /// <summary>
        /// Returns all types in an assembly.
        /// 
        /// Thanks to <see href="https://stackoverflow.com/a/29379834/10614206"/>
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null).Select(t => t!);
            }
        }
    }
}
