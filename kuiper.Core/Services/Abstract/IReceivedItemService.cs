using System;
using System.Collections.Generic;
using System.Text;

using kbo.littlerocks;

namespace kuiper.Core.Services.Abstract
{
    public interface IReceivedItemService
    {
        Task AddReceivedItemAsync(long receivingSlot, long sendingSlot, NetworkItem item);
        Task<IEnumerable<(NetworkItem, long)>> GetReceivedItemsAsync(long slot);
    }
}
