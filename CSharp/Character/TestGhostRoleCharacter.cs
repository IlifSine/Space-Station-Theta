//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class TestGhostRoleCharacter : Node3D
{
	[Export] Camera3D Camera;
	string GhostManagerPath = "/root/GameWorld/GhostManager";
	GhostManager ghostManager = new GhostManager();
	
	public override void _Ready()
	{
		ghostManager = GetNode<GhostManager>(GhostManagerPath);
		if (Multiplayer.IsServer())
		{
			ghostManager.AddGhostRole("Test Role", "Coder is testing", GetPath());
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void ChangeOwner(int PlayerId)
	{
		SetMultiplayerAuthority(PlayerId);
		RefreshAuthority();
	}

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
