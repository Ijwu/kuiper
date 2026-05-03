using System.Collections.Concurrent;
using System.Text.Json.Nodes;

using BigFloatLibrary;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Constants;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.DataStorage.Plugins
{
    public class DataStorageSetPlugin : BasePlugin
    {
        private readonly INotifyingStorageService _storage;
        private readonly ConcurrentDictionary<string, HashSet<string>> _subscriptions = new(StringComparer.OrdinalIgnoreCase);

        public DataStorageSetPlugin(ILogger<DataStorageSetPlugin> logger, IConnectionManager connectionManager, INotifyingStorageService storage)
            : base(logger, connectionManager)
        {
            _storage = storage;
        }

        protected override void RegisterHandlers()
        {
            Handle<SetNotify>(HandleSetNotifyAsync);
            Handle<Set>(HandleSetAsync);
        }

        private async Task HandleSetNotifyAsync(SetNotify notify, string connectionId)
        {
            if (notify.Keys.Length > 0)
            {
                var stored = await _storage.LoadAsync<string[]>(StorageKeys.SetNotify(connectionId)) ?? [];
                var combined = new HashSet<string>(stored, StringComparer.OrdinalIgnoreCase);
                foreach (var key in notify.Keys)
                {
                    combined.Add(key);
                }

                _subscriptions[connectionId] = combined;
                await _storage.SaveAsync(StorageKeys.SetNotify(connectionId), combined.ToArray(), -1);
            }
            else
            {
                _subscriptions.TryRemove(connectionId, out _);
                await _storage.DeleteAsync(StorageKeys.SetNotify(connectionId), -1);
            }
        }

        private async Task HandleSetAsync(Set setPacket, string connectionId)
        {
            (bool success, long slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
            {
                Logger.LogDebug("Attempt to Set from unmapped connection {ConnectionId}.", connectionId);
                return;
            }

            if (setPacket.Key.StartsWith("_") || setPacket.Key.StartsWith("#"))
            {
                Logger.LogDebug("Attempt to Set protected key {Key} from connection {ConnectionId}.", setPacket.Key, connectionId);
                return;
            }

            JsonNode? existing = await _storage.LoadAsync<JsonNode>(setPacket.Key);
            JsonNode defaultNode = setPacket.Default;
            JsonNode originalNode = existing ?? defaultNode;

            JsonNode valueNode = existing ?? defaultNode;
            foreach (DataStorageOperation op in setPacket.Operations ?? [])
            {
                valueNode = ApplyOperation(valueNode, op, defaultNode);
            }

            await _storage.SaveAsync(setPacket.Key, valueNode, slotId);

            if (setPacket.WantReply)
            {
                await SendToConnectionAsync(connectionId, new SetReply(setPacket.Key, valueNode, originalNode, slotId));
            }

            await NotifySubscribersAsync(setPacket.Key, valueNode, originalNode, slotId);
        }

        private static JsonNode ApplyOperation(JsonNode current, DataStorageOperation op, JsonNode defaultNode)
        {
            return op switch
            {
                Replace r       => r.Value,
                Default         => current ?? defaultNode,
                Add add         => ApplyNumericOp(current, add.Value, (a, b) => a + b),
                Mul mul         => ApplyNumericOp(current, mul.Value, (a, b) => a * b),
                Pow pow         => ApplyPow(current, pow.Value),
                Mod mod         => ApplyNumericOp(current, mod.Value, (a, b) => b.IsZero ? a : a % b),
                Floor           => ApplyNumericUnaryOp(current, BigFloat.Floor),
                Ceil            => ApplyNumericUnaryOp(current, BigFloat.Ceiling),
                Max max         => ApplyNumericOp(current, max.Value, (a, b) => a > b ? a : b),
                Min min         => ApplyNumericOp(current, min.Value, (a, b) => a < b ? a : b),
                And and         => ApplyBitwiseOp(current, and.Value, (a, b) => a & b),
                Or or           => ApplyBitwiseOp(current, or.Value, (a, b) => a | (long)b),
                Xor xor         => ApplyBitwiseOp(current, xor.Value, (a, b) => a ^ b),
                LeftShift ls    => ApplyBitwiseOp(current, ls.Value, (a, b) => a << b),
                RightShift rs   => ApplyBitwiseOp(current, rs.Value, (a, b) => a >> b),
                Remove rem      => ApplyRemove(current, rem.Value),
                Pop pop         => ApplyPop(current, pop.Value),
                Update upd      => ApplyUpdate(current, upd.Value),
                _               => current
            };
        }

        /// <summary>
        /// Applies a binary numeric operation using BigFloat for arbitrary precision.
        /// </summary>
        private static JsonNode ApplyNumericOp(JsonNode current, JsonNode operand, Func<BigFloat, BigFloat, BigFloat> op)
        {
            if (!TryToBigFloat(current, out var a)) return current;
            if (!TryToBigFloat(operand, out var b)) return current;
            return BigFloatToNode(op(a, b));
        }

        /// <summary>
        /// Applies a unary numeric operation using BigFloat for arbitrary precision.
        /// </summary>
        private static JsonNode ApplyNumericUnaryOp(JsonNode current, Func<BigFloat, BigFloat> op)
        {
            if (!TryToBigFloat(current, out var a)) return current;
            return BigFloatToNode(op(a));
        }

        /// <summary>
        /// Applies the Pow operation. BigFloat.Pow requires an integer exponent; fractional
        /// exponents are not supported and leave the value unchanged.
        /// </summary>
        private static JsonNode ApplyPow(JsonNode current, JsonNode exponent)
        {
            if (!TryToBigFloat(current, out var baseValue)) return current;
            if (!TryToLong(exponent, out var exp) || exp < int.MinValue || exp > int.MaxValue) return current;
            return BigFloatToNode(BigFloat.Pow(baseValue, (int)exp));
        }

        /// <summary>
        /// Applies a bitwise operation. Bitwise operations work on 64-bit integers.
        /// </summary>
        private static JsonNode ApplyBitwiseOp(JsonNode current, JsonNode operand, Func<long, int, long> op)
        {
            if (!TryToLong(current, out var a)) return current;
            if (!TryToLong(operand, out var b) || b < int.MinValue || b > int.MaxValue) return current;
            return JsonValue.Create(op(a, (int)b))!;
        }

        /// <summary>
        /// Removes a key from a JSON object. Has no effect on other value types.
        /// </summary>
        private static JsonNode ApplyRemove(JsonNode current, JsonNode key)
        {
            if (current is JsonObject obj)
            {
                obj.Remove(key.ToString());
            }
            return current;
        }

        /// <summary>
        /// Removes the last element from a JSON array, or a named key from a JSON object.
        /// </summary>
        private static JsonNode ApplyPop(JsonNode current, JsonNode key)
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
                obj.Remove(key.ToString());
            }
            return current;
        }

        /// <summary>
        /// Merges the properties of a JSON object into the current JSON object.
        /// </summary>
        private static JsonNode ApplyUpdate(JsonNode current, JsonNode updates)
        {
            if (current is JsonObject obj && updates is JsonObject updatesObj)
            {
                foreach (var kvp in updatesObj)
                {
                    obj[kvp.Key] = kvp.Value?.DeepClone();
                }
            }
            return current;
        }

        /// <summary>
        /// Converts a BigFloat to a JSON string node for storage.
        /// </summary>
        private static JsonNode BigFloatToNode(BigFloat value)
        {
            return JsonValue.Create(value.ToString())!;
        }

        /// <summary>
        /// Attempts to parse a JSON node as a BigFloat for numeric operations.
        /// Supports numeric values, and string values that represent numbers.
        /// </summary>
        private static bool TryToBigFloat(JsonNode? node, out BigFloat value)
        {
            if (node is JsonValue jv)
            {
                if (jv.TryGetValue<string>(out var s))
                {
                    try { value = new BigFloat(s); return true; }
                    catch (FormatException) { }
                }
                if (jv.TryGetValue<double>(out var d)) { value = new BigFloat(d); return true; }
                if (jv.TryGetValue<long>(out var l)) { value = new BigFloat(l); return true; }
                if (jv.TryGetValue<int>(out var i)) { value = new BigFloat(i); return true; }
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to extract a long integer from a JSON node.
        /// Supports numeric values and string values that represent integers.
        /// </summary>
        private static bool TryToLong(JsonNode? node, out long value)
        {
            if (node is JsonValue jv)
            {
                if (jv.TryGetValue<long>(out var l)) { value = l; return true; }
                if (jv.TryGetValue<int>(out var i)) { value = i; return true; }
                if (jv.TryGetValue<string>(out var s) && long.TryParse(s, out var parsed)) { value = parsed; return true; }
            }
            value = 0;
            return false;
        }

        private async Task NotifySubscribersAsync(string key, JsonNode value, JsonNode originalValue, long slot)
        {
            var setReply = new SetReply(key, value, originalValue, slot);

            foreach (var (connectionId, subscribedKeys) in _subscriptions)
            {
                if (subscribedKeys.Contains(key))
                {
                    await ConnectionManager.SendJsonToConnectionAsync(connectionId, new Packet[] { setReply });
                }
            }
        }
    }
}

