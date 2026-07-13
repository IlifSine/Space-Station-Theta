//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class TestGhostRoleCharacter : Node3D
{
	[Export] private Camera3D Camera;
	private string GhostManagerPath = "/root/GameWorld/GhostManager";
	private GhostManager ghostManager = new GhostManager();
	
	public override void _Ready()
	{
		ghostManager = GetNode<GhostManager>(GhostManagerPath);
		if (Multiplayer.IsServer())
		{
			ghostManager.AddGhostRole("Test Role", "Coder is testing", GetPath());
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ChangeOwner(int PlayerId)
	{
		SetMultiplayerAuthority(PlayerId);
		RefreshAuthority();
	}

	private void RefreshAuthority()
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
