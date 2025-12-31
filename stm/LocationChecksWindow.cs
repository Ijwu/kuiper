using kbo.bigrocks;
using kbo.littlerocks;

using spaceport.schematics;
using Terminal.Gui;

namespace stm;

public class LocationChecksWindow : Window
{
    private readonly Connected _connected;
    private readonly DataPackage? _currentDataPackage;
    private readonly ITalkToTheServer _freighter;

    public LocationChecksWindow(Connected connected, DataPackage? currentDataPackage, ITalkToTheServer freighter)
    {
        _connected = connected;
        _currentDataPackage = currentDataPackage;
        _freighter = freighter;
        SetupInterface();
    }

    private void SetupInterface()
    {
        ColorScheme = Colors.ColorSchemes["Base"];
        Title = "Location Checks";

        int width = GetLengthOfLongestLocationName();
        Width = 80;
        Height = 20;

        var scrollView = new ScrollView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            AutoHideScrollBars = false,
            ContentSize = new Size(width, _connected.MissingLocations.Length),
            ShowVerticalScrollIndicator = true,
            ShowHorizontalScrollIndicator = true
        };

        int y = 1;
        foreach (long id in _connected.MissingLocations)
        {
            scrollView.Add(CreateButtonForLocationCheckById(id, 1, y++));
        }

        Add(scrollView);
    }

    private View CreateButtonForLocationCheckById(long id, int x, int y)
    {
        var button = new Button(GetLocationNameFromId(id))
        {
            X = x,
            Y = y
        };

        button.Clicked += () => {
            SendLocationChecked(id);
            button.SuperView.Remove(button);
        };

        return button;
    }

    private async void SendLocationChecked(long id)
    {
        await _freighter.SendPacketsAsync([new LocationChecks([id])]);
    }

    private string GetLocationNameFromId(long id)
    {
        string currentGame = ((NetworkSlot)_connected.SlotInfo[_connected.Slot]).Game;
        return _currentDataPackage!.Data.Games[currentGame].LocationNameToId.ToDictionary(kv => kv.Value, kv => kv.Key).TryGetValue(id, out var locationName)
            ? locationName
            : id.ToString();
    }

    private int GetLengthOfLongestLocationName()
    {
        string currentGame = ((NetworkSlot)_connected.SlotInfo[_connected.Slot]).Game;
        return _currentDataPackage!.Data.Games[currentGame].LocationNameToId.Max(x => x.Key.Length);
    }
}
