using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace YetAnotherBot
{
    public class PluginResolver<PluginInterface>
    {
        public class PluginResolvedEventArgs<T>
        {
            public T Plugin { get; protected set; }
            public Assembly Assembly { get; protected set; }

            public PluginResolvedEventArgs(T Plugin, Assembly Assembly)
            {
                this.Plugin = Plugin;
                this.Assembly = Assembly;
            }
        }

        public class PluginResolveErrorEventArgs
        {
            public Exception Exception { get; protected set; }
            public Assembly Assembly { get; protected set; }

            public PluginResolveErrorEventArgs(Exception Exception, Assembly Assembly)
            {
                this.Exception = Exception;
                this.Assembly = Assembly;
            }
        }

        /// <summary>
        /// This event fires when a plugin failed to resolve
        /// </summary>
        public event EventHandler<PluginResolveErrorEventArgs> PluginResolveError;
        /// <summary>
        /// This event fires when a plugin successfully resolved
        /// </summary>
        public event EventHandler<PluginResolvedEventArgs<PluginInterface>> PluginResolved;

        /// <summary>
        /// Load plugins from a directory and fires events
        /// </summary>
        /// <param name="directory">Directory path to look for plugins</param>
        /// <param name="PluginConstructor">Constructor for the plugins</param>
        public void ResolvePlugins(string directory, params object[] PluginConstructor)
        {
            Assembly assembly = null;
            try
            {
                if (Directory.Exists(directory))
                {
                    foreach (var file in Directory.GetFiles(directory, "*.dll"))
                    {
                        try
                        {
                            byte[] assemblyData = File.ReadAllBytes(file);
                            assembly = Assembly.Load(assemblyData);
                        }
                        catch (Exception ex) { PluginResolveError?.Invoke(this, new PluginResolveErrorEventArgs(ex, assembly)); }
                        if (assembly != null)
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                if (type.IsInterface || type.IsAbstract)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (type.GetInterface(typeof(PluginInterface).FullName) != null)
                                    {
                                        PluginResolved?.Invoke(this, new PluginResolvedEventArgs<PluginInterface>((PluginInterface)Activator.CreateInstance(type, PluginConstructor), assembly));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PluginResolveError?.Invoke(this, new PluginResolveErrorEventArgs(ex, assembly));
            }
        }

        /// <summary>
        /// Loads a plugin from a path and fires events
        /// </summary>
        /// <param name="path">Path for the plugin</param>
        /// <param name="PluginConstructor">Plugin constructor</param>
        public void ResolvePlugin(string path, params object[] PluginConstructor)
        {
            Assembly assembly = null;
            try
            {
                if (File.Exists(path))
                {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);
                    assembly = Assembly.Load(assemblyName);
                    if (assembly != null)
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            if (type.IsInterface || type.IsAbstract)
                            {
                                continue;
                            }
                            else
                            {
                                if (type.GetInterface(typeof(PluginInterface).FullName) != null)
                                {
                                    PluginResolved?.Invoke(this, new PluginResolvedEventArgs<PluginInterface>((PluginInterface)Activator.CreateInstance(type, PluginConstructor), assembly));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PluginResolveError?.Invoke(this, new PluginResolveErrorEventArgs(ex, assembly));
            }
        }
    }
}
