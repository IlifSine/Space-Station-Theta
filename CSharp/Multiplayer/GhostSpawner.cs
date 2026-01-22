//Licensed under AGPL 3.0
using System.Linq;
using Godot;

public partial class GhostSpawner : Node
{
	[Export] private PackedScene GhostScene;

	public void SpawnGhost()
	{
		Rpc("RpcSpawnGhost", Multiplayer.GetUniqueId());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcSpawnGhost(int GhostPlayerId)
	{
		var GhostInstance = GhostScene.Instantiate();
		GhostInstance.SetMultiplayerAuthority(GhostPlayerId);
		GetNode<Node>("..").GetChildren().OfType<Node3D>().FirstOrDefault().AddChild(GhostInstance);
		GhostInstance.Name = GhostInstance.Name + GhostPlayerId;
	}
}
