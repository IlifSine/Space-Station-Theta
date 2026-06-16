//Licensed under AGPL 3.0. Glory to communism!
using System.Linq;
using Godot;

public partial class GhostSpawner : Node
{
	[Export] private PackedScene GhostScene;

	public void SpawnGhost()
	{
		Rpc("RpcSpawnGhost", Multiplayer.GetUniqueId());
	}

	public void DespawnGhost(int GhostPlayerId)
	{
		Rpc("RpcDespawnGhost", GhostPlayerId);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcSpawnGhost(int GhostPlayerId)
	{
		var GhostInstance = GhostScene.Instantiate();
		GhostInstance.SetMultiplayerAuthority(GhostPlayerId);
		GetNode<Node>("..").GetChildren().OfType<Node3D>().FirstOrDefault().AddChild(GhostInstance);
		GhostInstance.Name = GhostInstance.Name + GhostPlayerId;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcDespawnGhost(int GhostPlayerId)
	{
		foreach (GameMap item in GetNode<GameWorld>("/root/GameWorld").GetChildren().OfType<GameMap>())
		{
			foreach (Ghost ghost in item.GetChildren().OfType<Ghost>())
			{
				if (ghost.GetMultiplayerAuthority() == GhostPlayerId)
				{
					ghost.QueueFree();
					break;
				}
			}
		}
	}
}
