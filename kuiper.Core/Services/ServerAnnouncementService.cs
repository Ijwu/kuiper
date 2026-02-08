using System;
using System.Collections.Generic;
using System.Text;

using kuiper.Core.Services.Abstract;

namespace kuiper.Core.Services
{
    public class ServerAnnouncementService : IServerAnnouncementService
    {
        public Task AnnouncePlayerConnectedAsync(long slotId, string playerName)
        {
            throw new NotImplementedException();
        }

        public Task AnnouncePlayerDisconnectedAsync(long slotId, string playerName)
        {
            throw new NotImplementedException();
        }
    }
}
