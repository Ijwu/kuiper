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
    private readonly ITalkToTheServer _freighter;

    public LocationChecksWindow(RoomInfo? roomInfo, Connected connected, DataPackage? currentDataPackage, IReceiveTrade receiver, ITalkToTheServer freighter)
    {
        _roomInfo = roomInfo;
        _connected = connected;
        _currentDataPackage = currentDataPackage;
        _receiver = receiver;
        _freighter = freighter;
        SetupInterface();
    }

    private void SetupInterface()
    {
        ColorScheme = Colors.TopLevel;
        Title = "Location Checks";

        var scrollView = new ScrollView();

        foreach (long id in _connected.MissingLocations)
        {
            scrollView.Add(CreateButtonForLocationCheckById(id));
        }
    }

    private View CreateButtonForLocationCheckById(long id)
    {
        var button = new Button(GetLocationNameFromId(id));

        button.Clicked += () => SendLocationChecked(id);

        return button;
    }

    private async void SendLocationChecked(long id)
    {
        await _freighter.SendPacketsAsync([new LocationChecks([id])]);
    }

    private string GetLocationNameFromId(long id)
    {
        string currentGame = _connected.SlotInfo[_connected.Slot].Game;
        return _currentDataPackage!.Data.Games[currentGame].LocationNameToId.ToDictionary(kv => kv.Value, kv => kv.Key).TryGetValue(id, out var locationName)
            ? locationName
            : id.ToString();
    }
}
