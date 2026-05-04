using Godot;

public partial class TestGhostRoleCharacter : Node3D
{
	[Export] Camera3D Camera;
	string GameWorldPath = "/root/GameWorld";
	GameWorld gameWorld = new GameWorld();
	bool Authority;
	public override void _Ready()
	{
		//DEBUG
		GD.Print("sss");
		gameWorld = GetNode<GameWorld>(GameWorldPath);
		if (Multiplayer.IsServer())
		{
			//DEBUG
			GD.Print("ddd");
			gameWorld.AddGhostRole("Test Role", "Coder is testing");
			gameWorld.PrintGhostRoles();
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
