using System;
using System.Collections.Generic;
using System.Text;

using kuiper.Core.Constants;
using kuiper.Core.Services.Abstract;

namespace kuiper.Core.Extensions
{
    public static class INotifyingStorageServiceExtensions
    {
        public static async Task<bool> IsSlotAuthorizedAsync(this INotifyingStorageService self, long slotId)
        {
            long[] authorized = (await self.LoadAsync<long[]>(StorageKeys.AuthorizedCommandSlots)) ?? [];
            return authorized.Contains(slotId);
        }
    }
}
