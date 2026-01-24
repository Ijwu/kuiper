using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Constants;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class DataStorageSetPlugin : BasePlugin
    {
        private readonly IStorageService _storage;
        private readonly ConcurrentDictionary<string, HashSet<string>> _subscriptions = new(StringComparer.OrdinalIgnoreCase);

        public DataStorageSetPlugin(ILogger<DataStorageSetPlugin> logger, WebSocketConnectionManager connectionManager, IStorageService storage)
            : base(logger, connectionManager)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void RegisterHandlers()
        {
            Handle<SetNotify>(HandleSetNotifyAsync);
            Handle<Set>(HandleSetAsync);
        }

        private string NotifyKey(string connectionId) => StorageKeys.SetNotify(connectionId);

        private async Task HandleSetNotifyAsync(SetNotify notify, string connectionId)
        {
            if (notify.Keys is { Length: > 0 })
            {
                var stored = await _storage.LoadAsync<string[]>(NotifyKey(connectionId)) ?? Array.Empty<string>();
                var combined = new HashSet<string>(stored, StringComparer.OrdinalIgnoreCase);
                foreach (var k in notify.Keys)
                {
                    combined.Add(k);
                }

                _subscriptions[connectionId] = combined;
                await _storage.SaveAsync(NotifyKey(connectionId), combined.ToArray());
            }
            else
            {
                _subscriptions.TryRemove(connectionId, out _);
                await _storage.DeleteAsync(NotifyKey(connectionId));
            }
        }

        private async Task HandleSetAsync(Set setPacket, string connectionId)
        {
            var existing = await _storage.LoadAsync<JsonNode>(setPacket.Key);
            var defaultNode = setPacket.Default;
            var originalNode = existing ?? defaultNode;

            var valueNode = existing ?? defaultNode;
            foreach (var op in setPacket.Operations ?? Array.Empty<DataStorageOperation>())
            {
                valueNode = ApplyOperation(valueNode, op, defaultNode);
            }

            await _storage.SaveAsync(setPacket.Key, valueNode);

            var slot = await ConnectionManager.GetSlotForConnectionAsync(connectionId) ?? 0;

            if (setPacket.WantReply)
            {
                var reply = new SetReply(setPacket.Key, valueNode, originalNode, slot);
                await SendToConnectionAsync(connectionId, reply);
            }

            await NotifySubscribersAsync(setPacket.Key, valueNode, originalNode, slot);
        }

        private JsonNode ApplyOperation(JsonNode current, DataStorageOperation op, JsonNode defaultNode)
        {
            return op switch
            {
                Replace r => ToNode(r.Value),
                Default => current ?? defaultNode,
                Add add => ApplyNumeric(current, add.Value, (a, b) => a + b),
                Mul mul => ApplyNumeric(current, mul.Value, (a, b) => a * b),
                Pow pow => ApplyNumeric(current, pow.Value, (a, b) => Math.Pow(a, b)),
                Mod mod => ApplyNumeric(current, mod.Value, (a, b) => b == 0 ? a : a % b),
                Floor => ApplyNumeric(current, 0, (a, _) => Math.Floor(a)),
                Ceil => ApplyNumeric(current, 0, (a, _) => Math.Ceiling(a)),
                Max max => ApplyNumeric(current, max.Value, Math.Max),
                Min min => ApplyNumeric(current, min.Value, Math.Min),
                And and => ApplyBitwise(current, and.Value, (a, b) => a & b),
                Or or => ApplyBitwise(current, or.Value, (a, b) => a | b),
                Xor xor => ApplyBitwise(current, xor.Value, (a, b) => a ^ b),
                LeftShift ls => ApplyBitwise(current, ls.Value, (a, b) => a << b),
                RightShift rs => ApplyBitwise(current, rs.Value, (a, b) => a >> b),
                Remove rem => ApplyRemove(current, rem.Value),
                Pop pop => ApplyPop(current, pop.Value),
                Update upd => ApplyUpdate(current, upd.Value),
                _ => current
            };
        }

        private JsonNode ApplyNumeric(JsonNode current, object operand, Func<double, double, double> op)
        {
            if (!TryToDouble(current, out var a)) return current;
            if (!TryToDouble(operand, out var b)) return current;
            return JsonValue.Create(op(a, b))!;
        }

        private JsonNode ApplyBitwise(JsonNode current, object operand, Func<long, int, long> op)
        {
            if (!TryToLong(current, out var a)) return current;
            if (!TryToInt(operand, out var b)) return current;
            return JsonValue.Create(op(a, b))!;
        }

        private JsonNode ApplyRemove(JsonNode current, object key)
        {
            if (current is JsonObject obj)
            {
                obj.Remove(key?.ToString() ?? string.Empty);
            }
            return current;
        }

        private JsonNode ApplyPop(JsonNode current, object key)
        {
            if (current is JsonArray arr)
            {
                if (arr.Count > 0)
                {
                    arr.RemoveAt(arr.Count - 1);
                }
                return arr;
            }

            if (current is JsonObject obj)
            {
                obj.Remove(key?.ToString() ?? string.Empty);
            }
            return current;
        }

        private JsonNode ApplyUpdate(JsonNode current, object updates)
        {
            if (current is JsonObject obj)
            {
                if (updates is JsonObject updatesObj)
                {
                    foreach (var kvp in updatesObj)
                    {
                        obj[kvp.Key] = kvp.Value?.DeepClone();
                    }
                }
                else if (updates is IDictionary<string, object> sdict)
                {
                    foreach (var kvp in sdict)
                    {
                        obj[kvp.Key] = ToNode(kvp.Value);
                    }
                }
                else if (updates is IDictionary<object, object> dict)
                {
                    foreach (var kvp in dict)
                    {
                        obj[kvp.Key?.ToString() ?? string.Empty] = ToNode(kvp.Value);
                    }
                }
            }
            return current;
        }

        private static JsonNode ToNode(object? value)
        {
            return JsonSerializer.SerializeToNode(value) ?? JsonValue.Create(value)!;
        }

        private async Task NotifySubscribersAsync(string key, JsonNode value, JsonNode originalValue, long slot)
        {
            var setReply = new SetReply(key, value, originalValue, slot);

            foreach (var kvp in _subscriptions)
            {
                if (kvp.Value.Contains(key))
                {
                    await ConnectionManager.SendJsonToConnectionAsync(kvp.Key, new Packet[] { setReply });
                }
            }
        }

        private static bool TryToDouble(JsonNode input, out double value)
        {
            if (input is JsonValue jv)
            {
                if (jv.TryGetValue<double>(out var d)) { value = d; return true; }
                if (jv.TryGetValue<long>(out var l)) { value = l; return true; }
                if (jv.TryGetValue<int>(out var i)) { value = i; return true; }
                if (jv.TryGetValue<decimal>(out var m)) { value = (double)m; return true; }
                if (jv.TryGetValue<string>(out var s) && double.TryParse(s, out var parsed)) { value = parsed; return true; }
            }
            value = 0;
            return false;
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
                case JsonNode node when TryToDouble(node, out var v): value = v; return true;
                default:
                    value = 0;
                    return false;
            }
        }

        private static bool TryToLong(JsonNode input, out long value)
        {
            if (input is JsonValue jv)
            {
                if (jv.TryGetValue<long>(out var l)) { value = l; return true; }
                if (jv.TryGetValue<int>(out var i)) { value = i; return true; }
                if (jv.TryGetValue<string>(out var s) && long.TryParse(s, out var parsed)) { value = parsed; return true; }
            }
            value = 0;
            return false;
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
                case JsonNode node when TryToLong(node, out var v) && v >= int.MinValue && v <= int.MaxValue: value = (int)v; return true;
                default:
                    value = 0;
                    return false;
            }
        }
    }
}
