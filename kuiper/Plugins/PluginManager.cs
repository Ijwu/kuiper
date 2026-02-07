using kbo.littlerocks;

namespace kuiper.Plugins
{
    internal class PluginManager
    {
        private readonly List<IKuiperPlugin> _plugins;
        private readonly ILogger<PluginManager> _logger;
        private readonly SemaphoreSlim _semaphore;
        private readonly TimeSpan _perPluginTimeout;

        /// <summary>
        /// Create a PluginManager.
        /// </summary>
        /// <param name="logger">Logger injected by DI.</param>
        /// <param name="pluginInstances">Plugin instances to manage. Expected to be injected by DI as a list of all registered <see cref="IKuiperPlugin"/>s.</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of plugins to execute concurrently. Defaults to Environment.ProcessorCount * 2.</param>
        /// <param name="perPluginTimeout">Timeout for each plugin's ReceivePacket. Defaults to 5 seconds if not specified (null).</param>
        public PluginManager(ILogger<PluginManager> logger, List<IKuiperPlugin> pluginInstances, int? maxDegreeOfParallelism = null, TimeSpan? perPluginTimeout = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var degree = maxDegreeOfParallelism.GetValueOrDefault(Math.Max(1, Environment.ProcessorCount * 2));
            _semaphore = new SemaphoreSlim(degree, degree);
            _perPluginTimeout = perPluginTimeout ?? TimeSpan.FromSeconds(5);
            _plugins = pluginInstances;
        }
        public Task BroadcastPacketAsync(Packet packet, string connectionId)
        {
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
