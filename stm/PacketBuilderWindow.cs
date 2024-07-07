using System.Reflection;

using kbo.littlerocks;

using spaceport.schematics;

using Terminal.Gui;

namespace stm;
// NOTE: WIP
public class PacketBuilderWindow : Window
{
    private List<Type>? _packetTypes;
    private View? _packetSelectionView;
    private readonly ITalkToTheServer _sender;

    public PacketBuilderWindow(ITalkToTheServer sender)
    {
        ColorScheme = Colors.ColorSchemes["TopLevel"];
        if (sender is null)
        {
            MessageBox.ErrorQuery("Packet Builder Window Error", "Something is fucking broken. This time it's my fault. Time to crash.", "Ok");
            throw new ArgumentNullException(nameof(sender));
        }

        _sender = sender;

        Title = "Packet Builder";
        Width = 120;
        Height = 40;

        Add(GetOrBuildPacketSelector());
    }

    private View GetOrBuildPacketSelector()
    {
        if (_packetSelectionView is not null)
        {
            return _packetSelectionView;
        }

        if (_packetTypes is null)
        {
            PopulatePacketTypeCache();
        }

        View container = new()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        Label selectorLabel = new("Packet Type:")
        {
            X = Pos.Center() - 30,
            Y = Pos.At(1)
        };
        ComboBox packetSelector = new()
        {
            Width = Dim.Sized(40),
            Height = Dim.Sized(20),
            X = Pos.Right(selectorLabel) + 1,
            Y = Pos.Top(selectorLabel),
            HideDropdownListOnClick = true
        };
        packetSelector.SetSource(_packetTypes);

        Button submit = new("Continue")
        {
            X = Pos.Right(packetSelector) + 1,
            Y = Pos.Top(selectorLabel)
        };

        container.Add(selectorLabel, packetSelector, submit);

        _packetSelectionView = container;

        return container;
    }

    private void PopulatePacketTypeCache()
    {
        var allKbo = typeof(Packet).Assembly.GetTypes();
        _packetTypes = allKbo.Where(type => type.IsAssignableTo(typeof(Packet)) && type != typeof(Packet)).ToList();
    }

    private View CreatePacketBuilderForPacketType(Type packetType)
    {
        GetCurrentWidth(out var currWidth);
        GetCurrentHeight(out var currHeight);
        ScrollView container = new ScrollView()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            X = 0,
            Y = 0,
            AutoHideScrollBars = false,
            ContentSize = new Size(currWidth, currHeight),
            ShowVerticalScrollIndicator = true,
            ShowHorizontalScrollIndicator = true
        };

        throw new NotImplementedException();
    }
}
