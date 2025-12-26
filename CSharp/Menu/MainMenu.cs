//Licensed under AGPL 3.0
using Godot;

public partial class MainMenu : Control
{
	private BasicMultiplayerManager BMM;
	[Export] private string Address;
	[Export] private int Port = 8910;

	public override void _Ready()
	{
		BMM = GetNode<BasicMultiplayerManager>("../BasicMultiplayerManager");
	}

	public void JoinButtonPressed()
	{
		BMM.JoinGame(Address, Port);
	}
}
