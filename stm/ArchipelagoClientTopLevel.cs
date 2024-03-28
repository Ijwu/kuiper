using kbo.bigrocks;
using kbo.littlerocks;
using spaceport;
using spaceport.schematics;
using Terminal.Gui;

namespace stm;

public class ArchipelagoClientTopLevel : Toplevel
{
    private readonly Freighter _freighter = new Freighter();
    private readonly IReceiveTrade _receivingBay;
    private Window? _currentWindow;
    private IDisposable? _loginHandler;

    private RoomInfo? _currentRoomInfo;
    private DataPackage? _currentDataPackage;

    public ArchipelagoClientTopLevel()
    {
        _receivingBay = new ReceivingBay(_freighter);
        _currentWindow = new LoginWindow(OnLoginClickedAsync);

        Add(_currentWindow);
    }

    private async Task OnLoginClickedAsync()
    {
        if (_freighter.IsConnected)
        {
            return;
        }

        var loginWindow = _currentWindow as LoginWindow;

        if (loginWindow is null)
        {
            MessageBox.ErrorQuery("Login Error", "Something is fucking broken.", "Ok");
            return;
        }

        if (!loginWindow.Validate())
        {
            return;
        }

        string url = loginWindow.ServerUrl!;
        HookLoginHandler();
        await _freighter.ConnectAsync(new Uri(url));
    }

    private void HookLoginHandler()
    {
        async Task HandleLoggingInAsync(Packet[] packets)
        {
            var loginWindow = _currentWindow as LoginWindow;

            if (loginWindow is null)
            {
                MessageBox.ErrorQuery("Login Error", "Something is fucking broken.", "Ok");
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
                    
                }
                else if (packet is ConnectionRefused refused)
                {
                    MessageBox.ErrorQuery("Login Error", string.Join('\n', refused.Errors), "Ok");
                }
            }
        }

        _loginHandler = _receivingBay.OnPacketsReceived(HandleLoggingInAsync);
    }

    public async Task DisconnectAsync()
    {
        if (_loginHandler is not null)
        {
            _loginHandler.Dispose();
        }

        if (_freighter.IsConnected)
        {
            await _freighter.DisconnectAsync();
        }
    }
}
