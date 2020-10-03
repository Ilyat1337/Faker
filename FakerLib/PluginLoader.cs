using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FakerLib
{
    class PluginLoader
    {
        private static readonly string DLL_FILE_FILTER = "*.dll";

        internal static List<T> LoadPlugins<T>(string pluginsFolderName)
        {
            List<string> pluginPaths = GetPluginPaths(pluginsFolderName);
            List<Assembly> pluginsAssemblies = LoadPluginsAssemblies(pluginPaths);
            return CreatePlugins<T>(pluginsAssemblies);
        }

        private static List<string> GetPluginPaths(string pluginsFolderName)
        {
            return Directory.GetFiles(pluginsFolderName, DLL_FILE_FILTER).ToList();
        }

        private static List<Assembly> LoadPluginsAssemblies(List<string> pluginPaths)
        {
            List<Assembly> pluginsAssemblies = new List<Assembly>();
            foreach (string pluginPath in pluginPaths)
                pluginsAssemblies.Add(LoadPluginAssembly(pluginPath));
            return pluginsAssemblies;
        }

        private static Assembly LoadPluginAssembly(string pluginPath)
        {
            //    string root = Path.GetFullPath(Path.Combine(
            //Path.GetDirectoryName(
            //    Path.GetDirectoryName(
            //        Path.GetDirectoryName(
            //            Path.GetDirectoryName(
            //                Path.GetDirectoryName(typeof(PluginLoader).Assembly.Location)))))));

            //string pluginLocation = Path.GetFullPath(Path.Combine(root, pluginPath.Replace('\\', Path.DirectorySeparatorChar)));
            string pluginLocation = Path.GetFullPath(pluginPath.Replace('\\', Path.DirectorySeparatorChar));
            return Assembly.LoadFile(pluginLocation);
        }

        private static List<T> CreatePlugins<T>(List<Assembly> pluginsAssemblies)
        {
            List<T> pluginsList = new List<T>();
            foreach (Assembly pluginAssembly in pluginsAssemblies)
            {
                T plugin = TryCreatePlugin<T>(pluginAssembly);
                if (plugin != null)
                    pluginsList.Add(plugin);
            }
            return pluginsList;
        }

        private static T TryCreatePlugin<T>(Assembly pluginAssembly)
        {
            foreach (Type type in pluginAssembly.GetTypes())
            {
                if (typeof(T).IsAssignableFrom(type))
                {
                    T result = (T)Activator.CreateInstance(type);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return default;
        }
    }
}
