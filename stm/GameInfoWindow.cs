using kbo.bigrocks;

using Terminal.Gui;

namespace stm;

public class GameInfoWindow : Window
{
    private readonly RoomInfo? _roomInfo;

    public GameInfoWindow(RoomInfo? roomInfo)
    {
        _roomInfo = roomInfo;

        SetupInterface();
    }

    private void SetupInterface()
    {
        Title = "Game Info";
        Width = 80;
        Height = 20;

        if (_roomInfo is null)
        {
            MessageBox.ErrorQuery("Game Info Window Error", "Something is fucking broken. This time it's my fault.", "Ok");
            return;
        }

        var seedNameGroup = CreateInformationalGrouping("Seed Name", _roomInfo.SeedName);
        seedNameGroup.X = 1;
        seedNameGroup.Y = 1;
        Add(seedNameGroup);
    }

    private View CreateInformationalGrouping(string label, string value)
    {
        var container = new FrameView();

        var caption = new Label(label)
        {
            X = 0, Y = 0
        };

        var text = new Label(value)
        {
            X = caption.X + 1, Y = 0
        };

        container.Add(caption, text);
        return container;
    }
}
