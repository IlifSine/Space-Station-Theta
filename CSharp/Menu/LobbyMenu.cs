//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class LobbyMenu : Control
{
	private string GhostSpawnerPath = "/root/GameWorld/GhostSpawner";

	public override void _Ready()
	{
		GetNode<Label>("Label1").Text = Multiplayer.GetUniqueId().ToString();
	}

	public void SpectateButtonPressed()
	{
		GetNode<GhostSpawner>(GhostSpawnerPath).SpawnGhost();
	}
}
