using kuiper.Pickle;

namespace kuiper.Utilities
{
    public static class SlotResolver
    {
        public static bool TryResolveSlotId(string identifier, MultiData multiData, out long slotId)
        {
            slotId = -1;
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            // Try parsing as numeric ID first
            if (int.TryParse(identifier, out var id))
            {
                if (multiData.SlotInfo.ContainsKey(id))
                {
                    slotId = id;
                    return true;
                }
            }

            // Try matching by name (case-insensitive)
            var slotEntry = multiData.SlotInfo.FirstOrDefault(
                kvp => string.Equals(kvp.Value.Name, identifier, StringComparison.OrdinalIgnoreCase)
            );

            if (slotEntry.Value != null)
            {
                slotId = slotEntry.Key;
                return true;
            }

            return false;
        }
    }
}
