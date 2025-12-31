using System.Collections;
using System.IO.Compression;

using Razorvine.Pickle;

namespace kuiper.Pickle
{
    public static class MultidataUnpickler
    {
        public static MultiData Unpickle(Stream stream)
        {
            using BinaryReader reader = new BinaryReader(stream);
            var version = reader.ReadByte();

            Unpickler unpickler = new();

            using ZLibStream decompressed = new ZLibStream(stream, CompressionMode.Decompress);
            var unpickled = (Hashtable)unpickler.load(decompressed);

            MultiData md = new();
            md.SlotData = GetSlotData(unpickled["slot_data"]);
            md.SlotInfo = GetSlotInfo(unpickled["slot_info"]);
            md.ConnectNames = GetConnectNames(unpickled["connect_names"]);
            md.Locations = GetLocations(unpickled["locations"]);
            md.ChecksInArea = GetChecksInArea(unpickled["checks_in_area"]);
            md.ServerOptions = GetServerOptions(unpickled["server_options"]);
            md.ErHintData = GetErHintData(unpickled["er_hint_data"]);
            md.PrecollectedItems = GetPrecollectedItems(unpickled["precollected_items"]);
            md.PrecollectedHints = GetPrecollectedHints(unpickled["precollected_hints"]);
            md.Version = GetVersion(unpickled["version"]);
            md.Tags = Array.ConvertAll(((ArrayList)unpickled["tags"]).ToArray(), item => (string)item);
            md.MinimumVersions = GetMinimumVersions(unpickled["minimum_versions"]);
            md.SeedName = (string)unpickled["seed_name"];
            md.Spheres = GetSpheres(unpickled["spheres"]);
            md.DataPackage = GetDataPackage(unpickled["datapackage"]);
            md.RaceMode = (int)unpickled["race_mode"];
            return md;
        }

        private static Dictionary<string, GamesPackage> GetDataPackage(object unpickledDataPackage)
        {
            var ret = new Dictionary<string, GamesPackage>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledDataPackage)
            {
                var table = (Hashtable)kvp.Value;
                var itemNameGroups = new Dictionary<string, string[]>();
                var itemNameToId = new Dictionary<string, long>();
                var locationNameGroups = new Dictionary<string, string[]>();
                var locationNameToId = new Dictionary<string, long>();
                var itemNameGroupsTable = (Hashtable)table["item_name_groups"];
                foreach (DictionaryEntry kvp2 in itemNameGroupsTable)
                {
                    var value = Array.ConvertAll(((ArrayList)kvp2.Value).ToArray(), item => (string)item);
                    itemNameGroups.Add((string)kvp2.Key, value);
                }
                var itemNameToIdTable = (Hashtable)table["item_name_to_id"];
                foreach (DictionaryEntry kvp2 in itemNameToIdTable)
                {
                    itemNameToId.Add((string)kvp2.Key, (int)kvp2.Value);
                }
                var locationNameGroupsTable = (Hashtable)table["location_name_groups"];
                foreach (DictionaryEntry kvp2 in locationNameGroupsTable)
                {
                    var value = Array.ConvertAll(((ArrayList)kvp2.Value).ToArray(), item => (string)item);
                    locationNameGroups.Add((string)kvp2.Key, value);
                }
                var locationNameToIdTable = (Hashtable)table["location_name_to_id"];
                foreach (DictionaryEntry kvp2 in locationNameToIdTable)
                {
                    locationNameToId.Add((string)kvp2.Key, (int)kvp2.Value);
                }
                var checksum = (string)table["checksum"];
                ret.Add((string)kvp.Key, new GamesPackage
                {
                    ItemNameGroups = itemNameGroups,
                    ItemNameToId = itemNameToId,
                    LocationNameGroups = locationNameGroups,
                    LocationNameToId = locationNameToId,
                    Checksum = checksum
                });
            }
            return ret;
        }

        private static List<Dictionary<long, long[]>> GetSpheres(object unpickledSpheres)
        {
            var ret = new List<Dictionary<long, long[]>>();
            foreach (object sphereObj in (ArrayList)unpickledSpheres)
            {
                var sphereDict = new Dictionary<long, long[]>();
                var sphereTable = (Hashtable)sphereObj;
                foreach (DictionaryEntry kvp in sphereTable)
                {
                    var value = Array.ConvertAll(((HashSet<object>)kvp.Value).ToArray(), Convert.ToInt64);
                    sphereDict.Add((int)kvp.Key, value);
                }
                ret.Add(sphereDict);
            }
            return ret;
        }

        private static MinimumVersions GetMinimumVersions(object unpickledMinimumVersions)
        {
            var table = (Hashtable)unpickledMinimumVersions;
            var serverVersion = GetVersion(table["server"]);
            var clientsTable = (Hashtable)table["clients"];
            var clients = new Dictionary<long, Version>();
            foreach (DictionaryEntry kvp in clientsTable)
            {
                clients.Add((int)kvp.Key, GetVersion(kvp.Value));
            }
            return new MinimumVersions
            {
                Server = serverVersion,
                Clients = clients
            };
        }

        private static Version GetVersion(object unpickledVersion)
        {
            var array = (object[])unpickledVersion;
            var intArray = Array.ConvertAll(array, Convert.ToInt32);
            return new Version(intArray[0], intArray[1], intArray[2]);
        }

        private static Dictionary<long, MultiDataHint[]> GetPrecollectedHints(object unpickledPrecollectedHints)
        {
            var ret = new Dictionary<long, MultiDataHint[]>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledPrecollectedHints)
            {
                HashSet<object> hashset = (HashSet<object>)kvp.Value;
                var value = Array.ConvertAll(hashset.ToArray(), item => (MultiDataHint)item);//todo: this is probably wrong
                ret.Add((int)kvp.Key, value);
            }
            return ret;
        }

        private static Dictionary<long, long[]> GetPrecollectedItems(object unpickledPrecollectedItems)
        {
            var ret = new Dictionary<long, long[]>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledPrecollectedItems)
            {
                ArrayList list = (ArrayList)kvp.Value;
                var value = Array.ConvertAll(list.ToArray(), Convert.ToInt64);
                ret.Add((int)kvp.Key, value);
            }
            return ret;
        }

        private static Dictionary<long, Dictionary<long, string>> GetErHintData(object unpickledErHintData)
        {
            var ret = new Dictionary<long, Dictionary<long, string>>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledErHintData!)
            {
                var value = new Dictionary<long, string>();
                foreach (DictionaryEntry kvp2 in (Hashtable)kvp.Value)
                {
                    value.Add((long)kvp2.Key, (string)kvp2.Value);
                }
                ret.Add((long)kvp.Key, value);
            }
            return ret;
        }

        private static Dictionary<string, object> GetServerOptions(object unpickledServerOptions)
        {
            var ret = new Dictionary<string, object>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledServerOptions)
            {
                ret.Add((string)kvp.Key, kvp.Value);
            }
            return ret;
        }

        private static Dictionary<long, Dictionary<string, long[]>> GetChecksInArea(object unpickledChecksInArea)
        {
            var ret = new Dictionary<long, Dictionary<string, long[]>>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledChecksInArea)
            {
                var value = new Dictionary<string, long[]>();
                foreach (DictionaryEntry kvp2 in (Hashtable)kvp.Value)
                {
                    var value2 = Array.ConvertAll((object[])kvp2.Value, Convert.ToInt64);
                    value.Add((string)kvp2.Key, value2);
                }
                ret.Add((long)kvp.Key, value);
            }
            return ret;
        }

        private static Dictionary<long, Dictionary<long, long[]>> GetLocations(object unpickledLocations)
        {
            var ret = new Dictionary<long, Dictionary<long, long[]>>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledLocations)
            {
                var value = new Dictionary<long, long[]>();
                foreach (DictionaryEntry kvp2 in (Hashtable)kvp.Value)
                {
                    var value2 = Array.ConvertAll((object[])kvp2.Value, Convert.ToInt64);
                    value.Add((int)kvp2.Key, value2);
                }
                ret.Add((int)kvp.Key, value);
            }
            return ret;
        }

        private static Dictionary<string, long[]> GetConnectNames(object unpickledConnectNames)
        {
            var ret = new Dictionary<string, long[]>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledConnectNames)
            {
                var value = Array.ConvertAll((object[])kvp.Value, Convert.ToInt64);
                ret.Add((string)kvp.Key, value);
            }
            return ret;
        }

        private static Dictionary<int, MultiDataNetworkSlot> GetSlotInfo(object unpickledSlotInfo)
        {
            var ret = new Dictionary<int, MultiDataNetworkSlot>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledSlotInfo)
            {
                ret.Add((int)kvp.Key, (MultiDataNetworkSlot)kvp.Value);
            }
            return ret;
        }

        private static Dictionary<int, Dictionary<string, object>> GetSlotData(object unpickledSlotData)
        {
            var ret = new Dictionary<int, Dictionary<string, object>>();
            foreach (DictionaryEntry kvp in (Hashtable)unpickledSlotData)
            {
                int key = (int)kvp.Key;
                var value = new Dictionary<string, object>();
                foreach(DictionaryEntry kvp2 in (Hashtable)kvp.Value)
                {
                    string key2 = (string)kvp2.Key;
                    value.Add(key2, kvp2.Value);
                }
                ret.Add(key, value);
            }
            return ret;
        }
    }
}
