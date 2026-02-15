#nullable disable
using kbo.bigrocks;
using kbo.littlerocks;

namespace kuiper.Core.Pickle
{
    public class MultiData
    {
        public Dictionary<int, Dictionary<string, object>> SlotData { get; set; }
        public Dictionary<int, MultiDataNetworkSlot> SlotInfo { get; set; }
        public Dictionary<string, long[]> ConnectNames { get; set; }
        public Dictionary<long, Dictionary<long, long[]>> Locations { get; set; }
        public Dictionary<long, Dictionary<string, long[]>> ChecksInArea { get; set; }
        public Dictionary<string, object> ServerOptions { get; set; }
        public Dictionary<long, Dictionary<long, string>> ErHintData { get; set; }
        public Dictionary<long, long[]> PrecollectedItems { get; set; }
        public Dictionary<long, MultiDataHint[]> PrecollectedHints { get; set; }
        public Version Version { get; set; }
        public string[] Tags { get; set; }
        public MinimumVersions MinimumVersions { get; set; }
        public string SeedName { get; set; }
        public List<Dictionary<long, long[]>> Spheres { get; set; }
        public Dictionary<string, MultiDataGamesPackage> DataPackage { get; set; }
        public long RaceMode { get; set; }

        public DataPackage ToDataPackage()
        {
            var dataPackage = new DataPackage(new DataPackageContents(new()));
            foreach (var kvp in DataPackage)
            {
                var gameName = kvp.Key;
                var package = kvp.Value;
                var gamesPackage = new DataPackageGameData(package.ItemNameToId, package.LocationNameToId, package.Checksum);
                dataPackage.Data.Games[gameName] = gamesPackage;
            }
            return dataPackage;
        }
    }

    public record MultiDataGamesPackage
    {
        public Dictionary<string, string[]> ItemNameGroups { get; set; }
        public Dictionary<string, long> ItemNameToId { get; set; }
        public Dictionary<string, string[]> LocationNameGroups { get; set; }
        public Dictionary<string, long> LocationNameToId { get; set; }
        public string Checksum { get; set; }
    }

    public record MinimumVersions
    {
        public Version Server {  get; set; }
        public Dictionary<long, Version> Clients { get; set; }
    }

    public record MultiDataNetworkSlot
    {
        public string Name { get; set; }
        public string Game { get; set; }
        public MultiDataSlotType Type { get; set; }
        public long[] GroupMembers { get; set; }
    }

    public record MultiDataHint
    {
        public long ReceivingPlayer { get; set; }
        public long FindingPlayer { get; set; }
        public long Location { get; set; }
        public long Item { get; set; }
        public bool Found { get; set; }
        public string Entrance { get; set; }
        public long ItemFlags { get; set; }
        public MultiDataHintStatus Status { get; set; }
    }

    [Flags]
    public enum MultiDataSlotType
    {
        Spectator = 0,
        Player = 1,
        Group = 2
    }

    public enum MultiDataHintStatus: int
    {
        Unspecified = 0,
        NoPriority = 10,
        Avoid = 20,
        Priority = 30,
        Found = 40
    }
}
