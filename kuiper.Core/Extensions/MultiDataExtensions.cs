using kuiper.Core.Pickle;

namespace kuiper.Core.Extensions
{
    public static class MultiDataExtensions
    {
        /// <summary>
        /// Attempt to match the given identifier to a slot id in the <see cref="MultiData"/> object.
        /// It can interpret the given <paramref name="identifier"/> as a numerical slot id or as a slot name.
        /// </summary>
        /// <param name="multiData">The MultiData object to search for the slot.</param>
        /// <param name="identifier">The identifier to use to search the multidata.</param>
        /// <param name="slotId">The resolved slot id or null if no slot was resolved.</param>
        /// <returns>True if a slot id was resolved, false otherwise</returns>
        public static bool TryResolveSlotId(this MultiData multiData, string identifier, out long? slotId)
        {
            slotId = null;
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
