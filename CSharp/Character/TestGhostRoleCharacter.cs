using Godot;

public partial class TestGhostRoleCharacter : Node3D
{
	[Export] Camera3D Camera;
	string GameWorldPath = "/root/GameWorld";
	GameWorld gameWorld = new GameWorld();
	bool Authority;
	public override void _Ready()
	{
		gameWorld = GetNode<GameWorld>(GameWorldPath);
		if (Multiplayer.IsServer())
		{
			gameWorld.AddGhostRole("Test Role", "Coder is testing");
		}
	}

	void RefreshAuthority()
	{
		Authority = GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
		if (Authority)
		{
			Camera.MakeCurrent();
		}
		else
		{
			Camera.ClearCurrent();
		}
	}
}
