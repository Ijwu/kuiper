using kbo.littlerocks;

using spaceport.schematics;

using Terminal.Gui;

namespace stm;

public class PacketLogWindow : Window
{
    private readonly IReceiveTrade _bay;

    public PacketLogWindow(IReceiveTrade bay)
    {
        _bay = bay;

        SetupInterface();
    }

    private void SetupInterface()
    {
        Title = "Packet Log";
        Width = 80;
        Height = 20;

        ScrollView packetList = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            AutoHideScrollBars = false,
            ContentSize = new Size(400, 1),
            ShowVerticalScrollIndicator = true,
            ShowHorizontalScrollIndicator = true
        };

        void HandlePacketsForLogging(Packet[] packets)
        {
            foreach (Packet packet in packets)
            {
                Size newSize = new(packetList.ContentSize.Width, packetList.ContentSize.Height + 1);
                packetList.Add(new Label($"{DateTime.Now:t}: {packet}")
                {
                    X = 1,
                    Y = newSize.Height
                });
                packetList.ContentSize = newSize;
            }
        }
        _bay.OnPacketsReceived(HandlePacketsForLogging); // TODO: Dispose of this properly

        Add(packetList);
    }
}
