using System.Runtime.Serialization;

namespace kbo.littlerocks;

public enum ClientStatus
{
    ClientUnknown = 0,
    ClientConnected = 5,
    ClientReady = 10,
    ClientPlaying = 20,
    ClientGoal = 30
}
