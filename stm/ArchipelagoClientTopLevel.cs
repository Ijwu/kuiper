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
    private Window? _currentWindow;
    private IDisposable? _loginHandler;

    private RoomInfo? _currentRoomInfo;
    private DataPackage? _currentDataPackage;
    private Connected? _currentConnected;

    public ArchipelagoClientTopLevel()
    {
        _currentWindow = new LoginWindow(OnLoginClickedAsync);

        Add(_currentWindow);
    }

    private async void OnLoginClickedAsync()
    {
        if (_freighter.IsConnected)
        {
            MessageBox.ErrorQuery("Login Error", "You're already connected, dumdum.", "Ok");
            return;
        }

        var loginWindow = _currentWindow as LoginWindow;

        if (loginWindow is null)
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
            if (_currentWindow is not LoginWindow loginWindow)
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
                }
                else if (packet is DataPackage dataPackage)
                {
                    _currentDataPackage = dataPackage;
                    await _freighter.SendPacketsAsync([new Connect(
                        loginWindow.Password,
                        loginWindow.Game!, // Can't be null here, assumption is that validation has been completed.
                        loginWindow.SlotName!,
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
                    _loginHandler?.Dispose();
                    Remove(_currentWindow);
                    _currentWindow = new LocationChecksWindow(_currentRoomInfo, connected, _currentDataPackage, _receivingBay);
                    Add(_currentWindow);
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
