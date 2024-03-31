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
        Width = 40;
        Height = 20;

        if (_roomInfo is null)
        {
            MessageBox.ErrorQuery("Game Info Window Error", "Something is fucking broken. This time it's my fault.", "Ok");
            return;
        }

        var seedNameGroup = CreateInformationalGrouping("Seed Name", _roomInfo.SeedName, 0, 0);
        var serverTagsGroup = CreateInformationalGrouping("Server Tags", string.Join(", ", _roomInfo.Tags), 0, 1);
        var hintCostGroup = CreateInformationalGrouping("Hint Cost", _roomInfo.HintCost.ToString(), 0, 2);
        var locationCheckPointsGroup = CreateInformationalGrouping("Points Per Check", _roomInfo.LocationCheckPoints.ToString(), 0, 3);
        var serverVersionGroup = CreateInformationalGrouping("Server Version", _roomInfo.Version.ToString(), 0, 4);
        var generatorVersionGroup = CreateInformationalGrouping("Generator Version", _roomInfo.GeneratorVersion.ToString(), 0, 5);
        
        Add(seedNameGroup, serverTagsGroup, hintCostGroup, locationCheckPointsGroup, serverVersionGroup, generatorVersionGroup);
    }

    private View CreateInformationalGrouping(string label, string value, int x, int y)
    {
        var container = new View()
        {
            X = Pos.At(x),
            Y = Pos.At(y),
            Width = Dim.Fill(),
            Height = Dim.Sized(3)
        };

        var caption = new Label(0,0,label+":");

        var text = new Label(value)
        {
            X = Pos.Right(caption) + 1,
            Y = 0
        };

        container.Add(caption, text);
        return container;
    }
}
