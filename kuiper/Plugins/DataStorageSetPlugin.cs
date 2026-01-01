using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class DataStorageSetPlugin : IPlugin
    {
        private readonly ILogger<DataStorageSetPlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IStorageService _storage;
        private readonly ConcurrentDictionary<string, HashSet<string>> _subscriptions = new(StringComparer.OrdinalIgnoreCase);

        public DataStorageSetPlugin(ILogger<DataStorageSetPlugin> logger, WebSocketConnectionManager connectionManager, IStorageService storage)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            switch (packet)
            {
                case SetNotify notify:
                    HandleSetNotify(connectionId, notify);
                    break;
                case Set setPacket:
                    await HandleSetAsync(connectionId, setPacket);
                    break;
            }
        }

        private void HandleSetNotify(string connectionId, SetNotify notify)
        {
            if (notify.Keys is { Length: > 0 })
            {
                _subscriptions[connectionId] = notify.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                _subscriptions.TryRemove(connectionId, out _);
            }
        }

        private async Task HandleSetAsync(string connectionId, Set setPacket)
        {
            try
            {
                object? current = await _storage.LoadAsync<object>(setPacket.Key);
                var original = current ?? setPacket.Default;

                object value = current ?? setPacket.Default;

                foreach (var op in setPacket.Operations ?? Array.Empty<DataStorageOperation>())
                {
                    value = ApplyOperation(value, op, setPacket.Default);
                }

                await _storage.SaveAsync(setPacket.Key, value);

                var slot = await _connectionManager.GetSlotForConnectionAsync(connectionId) ?? 0;

                // respond to requester if asked
                if (setPacket.WantReply)
                {
                    var reply = new SetReply(setPacket.Key, value, original, slot);
                    await _connectionManager.SendJsonToConnectionAsync(connectionId, new Packet[] { reply });
                }

                await NotifySubscribersAsync(setPacket.Key, value, original, slot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle Set packet for key {Key}", setPacket.Key);
            }
        }

        private object ApplyOperation(object current, DataStorageOperation op, object @default)
        {
            switch (op)
            {
                case Replace r:
                    return r.Value;
                case Default:
                    return current ?? @default;
                case Add add:
                    return ApplyNumeric(current, add.Value, (a, b) => a + b);
                case Mul mul:
                    return ApplyNumeric(current, mul.Value, (a, b) => a * b);
                case Pow pow:
                    return ApplyNumeric(current, pow.Value, (a, b) => Math.Pow(a, b));
                case Mod mod:
                    return ApplyNumeric(current, mod.Value, (a, b) => b == 0 ? a : a % b);
                case Floor:
                    return ApplyNumeric(current, 0, (a, _) => Math.Floor(a));
                case Ceil:
                    return ApplyNumeric(current, 0, (a, _) => Math.Ceiling(a));
                case Max max:
                    return ApplyNumeric(current, max.Value, Math.Max);
                case Min min:
                    return ApplyNumeric(current, min.Value, Math.Min);
                case And and:
                    return ApplyBitwise(current, and.Value, (a, b) => a & b);
                case Or or:
                    return ApplyBitwise(current, or.Value, (a, b) => a | b);
                case Xor xor:
                    return ApplyBitwise(current, xor.Value, (a, b) => a ^ b);
                case LeftShift ls:
                    return ApplyBitwise(current, ls.Value, (a, b) => a << b);
                case RightShift rs:
                    return ApplyBitwise(current, rs.Value, (a, b) => a >> b);
                case Remove rem:
                    return ApplyRemove(current, rem.Value);
                case Pop pop:
                    return ApplyPop(current, pop.Value);
                case Update upd:
                    return ApplyUpdate(current, upd.Value);
                default:
                    return current;
            }
        }

        private object ApplyNumeric(object current, object operand, Func<double, double, double> op)
        {
            if (!TryToDouble(current, out var a)) return current;
            if (!TryToDouble(operand, out var b)) return current;
            var result = op(a, b);
            return CastBack(current, result);
        }

        private object ApplyBitwise(object current, object operand, Func<long, int, long> op)
        {
            if (!TryToLong(current, out var a)) return current;
            if (!TryToInt(operand, out var b)) return current;
            var result = op(a, b);
            return CastBack(current, result);
        }

        private object ApplyRemove(object current, object key)
        {
            switch (current)
            {
                case IDictionary<object, object> dict:
                    dict.Remove(key);
                    return current;
                case IDictionary<string, object> sdict:
                    if (key is string sk) sdict.Remove(sk);
                    return current;
            }
            return current;
        }

        private object ApplyPop(object current, object key)
        {
            switch (current)
            {
                case IList<object> list when list.Count > 0:
                    list.RemoveAt(list.Count - 1);
                    return current;
                case IDictionary<object, object> dict:
                    dict.Remove(key);
                    return current;
                case IDictionary<string, object> sdict when key is string sk:
                    sdict.Remove(sk);
                    return current;
            }
            return current;
        }

        private object ApplyUpdate(object current, object updates)
        {
            if (current is IDictionary<string, object> sdict && updates is IDictionary<string, object> supdates)
            {
                foreach (var kvp in supdates)
                {
                    sdict[kvp.Key] = kvp.Value;
                }
            }
            else if (current is IDictionary<object, object> dict && updates is IDictionary<object, object> updatesObj)
            {
                foreach (var kvp in updatesObj)
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }
            return current;
        }

        private async Task NotifySubscribersAsync(string key, object value, object originalValue, long slot)
        {
            var jsonNode = JsonSerializer.SerializeToNode(value) ?? JsonValue.Create(value);
            var setReply = new SetReply(key, value, originalValue, slot);

            foreach (var kvp in _subscriptions)
            {
                if (kvp.Value.Contains(key))
                {
                    await _connectionManager.SendJsonToConnectionAsync(kvp.Key, new Packet[] { setReply });
                }
            }
        }

        private static bool TryToDouble(object input, out double value)
        {
            switch (input)
            {
                case double d: value = d; return true;
                case float f: value = f; return true;
                case int i: value = i; return true;
                case long l: value = l; return true;
                case decimal m: value = (double)m; return true;
                case string s when double.TryParse(s, out var parsed): value = parsed; return true;
                default:
                    value = 0;
                    return false;
            }
        }

        private static bool TryToLong(object input, out long value)
        {
            switch (input)
            {
                case long l: value = l; return true;
                case int i: value = i; return true;
                case short s: value = s; return true;
                case byte b: value = b; return true;
                case string str when long.TryParse(str, out var parsed): value = parsed; return true;
                default:
                    value = 0;
                    return false;
            }
        }

        private static bool TryToInt(object input, out int value)
        {
            switch (input)
            {
                case int i: value = i; return true;
                case short s: value = s; return true;
                case byte b: value = b; return true;
                case long l when l >= int.MinValue && l <= int.MaxValue: value = (int)l; return true;
                case string str when int.TryParse(str, out var parsed): value = parsed; return true;
                default:
                    value = 0;
                    return false;
            }
        }

        private static object CastBack(object original, double result)
        {
            return original switch
            {
                int => (int)result,
                long => (long)result,
                float => (float)result,
                decimal => (decimal)result,
                _ => result
            };
        }

        private static object CastBack(object original, long result)
        {
            return original switch
            {
                int => (int)result,
                long => result,
                short => (short)result,
                byte => (byte)result,
                _ => result
            };
        }
    }
}
