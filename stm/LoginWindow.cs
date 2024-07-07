using Terminal.Gui;

namespace stm;

public class LoginWindow : Window
{
    private readonly TextField _slotText;
    private readonly TextField _urlText;
    private readonly TextField _passText;
    private readonly TextField _gameText;

    public string? SlotName => _slotText.Text.ToString();
    public string? ServerUrl => _urlText.Text.ToString();
    public string? Password => _passText.Text.ToString();
    public string? Game  => _gameText.Text.ToString();

    public LoginWindow(Action loginClicked)
    {
        ColorScheme = Colors.ColorSchemes["Base"];
        Width = 120;
        Height = 20;

        Title = "Login";

        var slotLabel = new Label() { 
			Text = "Slot Name:" 
		};

        _slotText = new TextField("1") {
			X = Pos.Right (slotLabel) + 1,
			Width = Dim.Fill(),
		};

        var gameLabel = new Label() { 
			Text = "Game:",
            X = Pos.Left(slotLabel),
			Y = Pos.Bottom(slotLabel) + 1
		};

        _gameText = new TextField("Blasphemous") {
			X = Pos.Left(_slotText),
			Y = Pos.Top(gameLabel),
			Width = Dim.Fill(),
		};

        var urlLabel = new Label() { 
			Text = "Server Url:",
            X = Pos.Left(gameLabel),
			Y = Pos.Bottom(gameLabel) + 1
		};

        _urlText = new TextField("ws://127.0.0.1:38281") {
			X = Pos.Left(_gameText),
			Y = Pos.Top(urlLabel),
			Width = Dim.Fill(),
		};

        var passLabel = new Label() { 
			Text = "Password (Leave empty if none):",
            X = Pos.Left(slotLabel),
			Y = Pos.Bottom(urlLabel) + 1
		};

        _passText = new TextField("") {
			X = Pos.Right(passLabel) + 1,
            Y = Pos.Top(passLabel),
			Width = Dim.Fill(),
		};

		var loginBtn = new Button() {
			Text = "Connect",
			Y = Pos.Bottom(passLabel) + 1,
			X = Pos.Center(),
			IsDefault = true,
		};

		loginBtn.Clicked += () => Application.MainLoop.Invoke(loginClicked);

        Add(slotLabel, _slotText, gameLabel, _gameText, urlLabel, _urlText, passLabel, _passText, loginBtn);
    }

    public bool Validate()
    {
        if (string.IsNullOrEmpty(SlotName))
        {
            MessageBox.ErrorQuery("Login Error", "Enter in a slot name, you dolt.", "Ok");
            return false;
        }

        if (string.IsNullOrEmpty(Game))
        {
            MessageBox.ErrorQuery("Login Error", "Enter in a game, you dingus.", "Ok");
            return false;
        }

        if (string.IsNullOrEmpty(ServerUrl))
        {
            MessageBox.ErrorQuery("Login Error", "Enter in the server url, you caveperson.", "Ok");
            return false;
        }

        return true;
    }
}
