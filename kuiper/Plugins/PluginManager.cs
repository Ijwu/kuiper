using kbo.littlerocks;

namespace kuiper.Plugins
{
    public class PluginManager
    {
        private readonly List<Type> _pluginTypes = new();
        private readonly List<IPlugin> _plugins = new();
        private readonly ILogger<PluginManager> _logger;
        private readonly SemaphoreSlim _semaphore;
        private readonly TimeSpan _perPluginTimeout;
        private bool _initialized;

        /// <summary>
        /// Create a PluginManager.
        /// </summary>
        /// <param name="logger">Logger injected by DI.</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of plugins to execute concurrently. Defaults to Environment.ProcessorCount * 2.</param>
        /// <param name="perPluginTimeout">Timeout for each plugin's ReceivePacket. Defaults to 5 seconds if not specified (null).</param>
        public PluginManager(ILogger<PluginManager> logger, int? maxDegreeOfParallelism = null, TimeSpan? perPluginTimeout = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var degree = maxDegreeOfParallelism.GetValueOrDefault(Math.Max(1, Environment.ProcessorCount * 2));
            _semaphore = new SemaphoreSlim(degree, degree);
            _perPluginTimeout = perPluginTimeout ?? TimeSpan.FromSeconds(5);
        }

        public void RegisterPlugin(Type pluginType)
        {
            if (pluginType == null) throw new ArgumentNullException(nameof(pluginType));
            if (!typeof(IPlugin).IsAssignableFrom(pluginType))
                throw new ArgumentException($"Type '{pluginType.FullName}' does not implement IPlugin.", nameof(pluginType));

            if (_pluginTypes.Contains(pluginType)) return;
            _pluginTypes.Add(pluginType);
        }

        public void RegisterPlugin<T>() where T : IPlugin
        {
            RegisterPlugin(typeof(T));
        }

        /// <summary>
        /// Activates instances of all registered plugin types and stores them for use by BroadcastPacketAsync.
        /// If an IServiceProvider is provided, it will be asked for the plugin instance first; otherwise
        /// Activator.CreateInstance will be used.
        /// </summary>
        public void Initialize(IServiceProvider? serviceProvider = null)
        {
            if (_initialized) return;

            foreach (var type in _pluginTypes)
            {
                object? instance = null;

                try
                {
                    if (serviceProvider != null)
                    {
                        instance = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type);
                    }

                    if (instance == null)
                    {
                        throw new InvalidOperationException($"Unable to create instance of '{type.FullName}'.");
                    }

                    var plugin = (IPlugin)instance;
                    _plugins.Add(plugin);
                    _logger.LogInformation("Initialized plugin instance of type {PluginType}", type.FullName);
                }
                catch (Exception ex)
                {
                    // Log and continue initializing other plugins. A single failing plugin type should not prevent others.
                    _logger.LogError(ex, "Failed to initialize plugin type '{PluginType}'", type.FullName);
                }
            }

            _initialized = true;
        }

        public Task BroadcastPacketAsync(Packet packet, string connectionId)
        {
            if (!_initialized)
                throw new InvalidOperationException("PluginManager has not been initialized. Call Initialize() after registering plugin types.");

            foreach (var plugin in _plugins.ToList())
            {
                _ = Task.Run(async () =>
                {
                    await _semaphore.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        var pluginTask = plugin.ReceivePacket(packet, connectionId);

                        var completed = await Task.WhenAny(pluginTask, Task.Delay(_perPluginTimeout)).ConfigureAwait(false);
                        if (!ReferenceEquals(completed, pluginTask))
                        {
                            _logger.LogWarning("Plugin '{PluginType}' timed out after {Timeout} while handling a packet from {ConnectionId}.", plugin.GetType().FullName, _perPluginTimeout, connectionId);
                            // Observe eventual exceptions from the plugin task to avoid unobserved exceptions
                            _ = pluginTask.ContinueWith(t =>
                            {
                                if (t.IsFaulted && t.Exception != null)
                                    _logger.LogError(t.Exception, "Plugin '{PluginType}' faulted after timeout.", plugin.GetType().FullName);
                            }, TaskScheduler.Default);
                        }
                        else
                        {
                            try
                            {
                                await pluginTask.ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Plugin '{PluginType}' failed to handle packet.", plugin.GetType().FullName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error while executing plugin '{PluginType}'.", plugin.GetType().FullName);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });
            }

            return Task.CompletedTask;
        }
    }
}
