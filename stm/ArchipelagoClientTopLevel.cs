using kbo.bigrocks;
using kbo.littlerocks;
using spaceport;
using spaceport.schematics;
using Terminal.Gui;

namespace stm;

public class ArchipelagoClientTopLevel : Toplevel
{
    private readonly Freighter _freighter = new Freighter();
    private IReceiveTrade? _receivingBay;
#pragma warning disable IDE0044 // Add readonly modifier - suppressed since one day it will be re-assigned
    private LoginWindow? _loginWindow;
#pragma warning restore IDE0044 // Add readonly modifier
    private LocationChecksWindow? _locationChecksWindow;
    private GameInfoWindow? _gameInfoWindow;
    private PacketLogWindow? _packetLogWindow;
    private readonly MenuBar _menuBar;    
    private IDisposable? _loginHandler;
    private RoomInfo? _currentRoomInfo;
    private DataPackage? _currentDataPackage;
    private Connected? _currentConnected;

    public ArchipelagoClientTopLevel()
    {
        ColorScheme = Colors.ColorSchemes["TopLevel"];
        _loginWindow = new LoginWindow(OnLoginClickedAsync)
        {
            X = Pos.Center(),
            Y = Pos.Center()
        };

        _menuBar = new MenuBar([
            new("_Application", (MenuItem[])[
                new("_Quit", "Quit the application.", () => Application.MainLoop.Invoke(async () => { await DisconnectAsync(); Application.RequestStop();}))
            ])
        ]);

        // var packetBuilder = new PacketBuilderWindow(_freighter){Y = 1};
        Add(_menuBar, _loginWindow);
        // Add(_menuBar, packetBuilder);
    }

    private MenuBarItem? CreateWindowsMenuBarItem()
    {
        if (_gameInfoWindow is null)
        {
            MessageBox.ErrorQuery("UI Error", "Something is fucking broken. This time it's my fault.", "Ok");
            return null;
        }

        if (_locationChecksWindow is null)
        {
            MessageBox.ErrorQuery("UI Error", "Something is fucking broken. This time it's my fault.", "Ok");
            return null;
        }

        return new("_Windows", (MenuItem[])[
                new("_Game Info", "Toggle Game Info Window", () => _gameInfoWindow.Visible = !_gameInfoWindow.Visible),
                new("_Location Checks", "Toggle Location Check Window", () => _locationChecksWindow.Visible = !_locationChecksWindow.Visible)
            ]);
    }

    private async void OnLoginClickedAsync()
    {
        if (_freighter.IsConnected)
        {
            MessageBox.ErrorQuery("Login Error", "You're already connected, dumdum.", "Ok");
            return;
        }

        if (_loginWindow is not LoginWindow loginWindow)
        {
            MessageBox.ErrorQuery("Login Error", "Something is fucking broken. This time it's my fault.", "Ok");
            return;
        }

        if (!loginWindow.Validate())
        {
            return;
        }

        string url = loginWindow.ServerUrl!;
        _receivingBay = new ReceivingBay(_freighter);
        HookLoginHandler();
        await _freighter.ConnectAsync(new Uri(url));
        _receivingBay.StartReceiving();
    }

    private void HookLoginHandler()
    {
        async Task HandleLoggingInAsync(Packet[] packets)
        {
            if (_loginWindow is null)
            {
                MessageBox.ErrorQuery("Login Error", "Something is fucking broken. This time it's my fault.", "Ok");
                return;
            }

            foreach (Packet packet in packets)
            {
                if (packet is RoomInfo roomInfo)
                {
                    _currentRoomInfo = roomInfo;
                    await _freighter.SendPacketsAsync([new GetDataPackage(roomInfo.Games)]);
                    _gameInfoWindow = new GameInfoWindow(_currentRoomInfo)
                    {
                        X = Pos.Left(this),
                        Y = Pos.Bottom(_menuBar)
                    };
                    Add(_gameInfoWindow);
                }
                else if (packet is DataPackage dataPackage)
                {
                    _currentDataPackage = dataPackage;
                    await _freighter.SendPacketsAsync([new Connect(
                        _loginWindow.Password,
                        _loginWindow.Game!, // Can't be null here, assumption is that validation has been completed.
                        _loginWindow.SlotName!,
                        Guid.NewGuid(),
                        new Version(5,0,0,0),
                        ItemHandlingFlags.All,
                        [],
                        false
                    )]);
                }
                else if (packet is Connected connected)
                {
                    _currentConnected = connected;
                    Remove(_loginWindow);
                    _locationChecksWindow = new LocationChecksWindow(_currentConnected, _currentDataPackage, _freighter)
                    {
                        X = Pos.Right(_gameInfoWindow),
                        Y = Pos.Bottom(_menuBar)
                    };
                    
                    _packetLogWindow = new PacketLogWindow(_receivingBay)
                    {
                        X = Pos.Right(_locationChecksWindow),
                        Y = Pos.Bottom(_menuBar)
                    };

                    Add(_locationChecksWindow, _packetLogWindow);
                    _menuBar.Menus = [.._menuBar.Menus, CreateWindowsMenuBarItem()];
                }
                else if (packet is ConnectionRefused refused)
                {
                    MessageBox.ErrorQuery("Login Error", string.Join('\n', refused.Errors), "Ok");
                }
            }
        }

        if (_receivingBay is not null)
        {
            _loginHandler = _receivingBay.OnPacketsReceived(HandleLoggingInAsync);
        }
    }

    public async Task DisconnectAsync()
    {
        _loginHandler?.Dispose();

        if (_freighter.IsConnected)
        {
            _receivingBay?.Dispose();
            await _freighter.DisconnectAsync();
        }
    }
}
