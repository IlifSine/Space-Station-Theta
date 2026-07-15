//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class MainMenu : Control
{
	private BasicMultiplayerManager BMM;
	[Export] private LineEdit inputPort;
	[Export] private LineEdit inputAddress;

	public override void _Ready()
	{
		BMM = GetNode<BasicMultiplayerManager>("../BasicMultiplayerManager");
	}

	public void JoinButtonPressed()
	{
		if (int.TryParse(inputPort.Text, out int port))
		{
			BMM.JoinGame(inputAddress.Text, port);
		}
	}
}
