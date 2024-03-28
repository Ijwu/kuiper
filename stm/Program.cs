using stm;
using Terminal.Gui;

Application.Init();

var apTopLevel = new ArchipelagoClientTopLevel();
Application.Top.Add(apTopLevel);
Application.Run();

await apTopLevel.DisconnectAsync();

Application.Shutdown();