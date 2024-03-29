using kbo.bigrocks;
using spaceport.schematics;
using Terminal.Gui;

namespace stm;

public class LocationChecksWindow : Window
{
    private readonly RoomInfo? _roomInfo;
    private readonly Connected _connected;
    private readonly DataPackage? _currentDataPackage;
    private readonly IReceiveTrade _receiver;

    public LocationChecksWindow(RoomInfo? roomInfo, Connected connected, DataPackage? currentDataPackage, IReceiveTrade receiver)
    {
        _roomInfo = roomInfo;
        _connected = connected;
        _currentDataPackage = currentDataPackage;
        _receiver = receiver;
    }
}
