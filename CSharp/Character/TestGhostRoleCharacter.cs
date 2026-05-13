//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class TestGhostRoleCharacter : Node3D
{
	[Export] Camera3D Camera;
	string GameWorldPath = "/root/GameWorld";
	GameWorld gameWorld = new GameWorld();
	
	public override void _Ready()
	{
		gameWorld = GetNode<GameWorld>(GameWorldPath);
		if (Multiplayer.IsServer())
		{
			gameWorld.AddGhostRole("Test Role", "Coder is testing", GetPath());
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void RefreshAuthority()
	{
		if (IsMultiplayerAuthority())
		{
			Camera.MakeCurrent();
		}
		else
		{
			Camera.ClearCurrent();
		}
	}
}
