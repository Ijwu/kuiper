using stm;
using Terminal.Gui;

Application.Init();

var appTopLevel = new ArchipelagoClientTopLevel();
Application.Top.Add(appTopLevel);
Application.Run();

await appTopLevel.DisconnectAsync();

Application.Shutdown();