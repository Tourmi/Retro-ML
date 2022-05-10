using Retro_ML.Plugin;
using System.Reflection;

namespace Retro_ML.Utils
{
    internal static class PluginUtils
    {
        private static readonly Dictionary<string, IPlugin> plugins = new();

        /// <summary>
        /// <br>Returns a <see cref="IGamePlugin"/> instance from the loaded plugins that's compatible with the given rom header name.</br>
        /// <br/>
        /// <br>Returns <c>null</c> if no plugins exist for the given game.</br>
        /// </summary>
        public static IGamePlugin? GetPluginForGame(string romHeaderName)
        {
            return GamePlugins.Where(p => p.PluginROMHeaderName == romHeaderName).FirstOrDefault();
        }
        /// <summary>
        /// Returns the plugin with the given name.
        /// </summary>
        public static T GetPlugin<T>(string pluginName) where T : IPlugin
        {
            return (T)plugins[pluginName];
        }

        /// <summary>
        /// Function that (re)loads all plugins in the <see cref="DefaultPaths.PLUGINS_DIR"/> directory.
        /// </summary>
        public static void LoadPlugins()
        {
            plugins.Clear();

            var pluginType = typeof(IPlugin);
            foreach (var file in new DirectoryInfo(DefaultPaths.PLUGINS_DIR).GetFiles("*.dll"))
            {
                try
                {
                    Assembly a = AppDomain.CurrentDomain.Load(Assembly.LoadFrom(file.FullName).GetName());
                    var foundTypes = GetLoadableTypes(a)
                        .Where(pluginType.IsAssignableFrom)
                        .Where(t => t.IsClass);
                    foreach (var foundType in foundTypes)
                    {
                        AddPluginInstance((IPlugin)Activator.CreateInstance(foundType)!);
                    }
                }
                catch
                {
                    Exceptions.QueueException(new Exception($"Could not load assembly {file.Name}, is it a valid plugin?"));
                }
            }
        }

        /// <summary>
        /// Returns all the loaded game plugins.
        /// </summary>
        public static IEnumerable<IGamePlugin> GamePlugins => plugins.Values.OfType<IGamePlugin>();
        /// <summary>
        /// Returns all the loaded console plugins.
        /// </summary>
        public static IEnumerable<IConsolePlugin> ConsolePlugins => plugins.Values.OfType<IConsolePlugin>();

        /// <summary>
        /// Adds the given plugin instance to the list of loaded plugins
        /// </summary>
        /// <param name="plugin"></param>
        /// <exception cref="Exception">Thrown when a plugin with the same name already exists</exception>
        private static void AddPluginInstance(IPlugin plugin)
        {
            if (plugins.ContainsKey(plugin.PluginName))
            {
                throw new Exception($"More than one plugin with name {plugin.PluginName} are present in the {DefaultPaths.PLUGINS_DIR} directory");
            }
            plugins[plugin.PluginName] = plugin;
        }

        /// <summary>
        /// Returns all types in an assembly.
        /// 
        /// Thanks to <see href="https://stackoverflow.com/a/29379834/10614206"/>
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
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
