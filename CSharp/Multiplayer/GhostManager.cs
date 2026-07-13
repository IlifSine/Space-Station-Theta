//Licensed under AGPL 3.0. Glory to communism!
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GhostManager : Node
{
	public List<GhostRoleData> GhostRoles = new List<GhostRoleData>();
	private List<Node3D> RoleNodes = new List<Node3D>();

	[Export] private PackedScene GhostScene;

	/// <summary>
	/// Adds a new ghost role to the available roles list on the server and broadcasts it to all clients.
	/// </summary>
	/// <param name="Name">The name of the ghost role</param>
	/// <param name="Desc">The description of the ghost role</param>
	/// <param name="Path">The node path to the role in the scene tree</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void AddGhostRole(string Name, string Desc, string Path)
	{
		if (Multiplayer.IsServer())
		{
			Rpc("LocalAddGhostRole", Name, Desc);
			RoleNodes.Add(GetNode<Node3D>(Path));
		}
	}

	/// <summary>
	/// Removes a ghost role from the available roles list on the server and broadcasts the removal to all clients.
	/// </summary>
	/// <param name="RoleId">The index of the role to remove</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RemoveGhostRole(int RoleId)
	{
		if (Multiplayer.IsServer())
		{
			Rpc("LocalRemoveGhostRole", RoleId);
			RoleNodes.RemoveAt(RoleId);
		}
	}

	/// <summary>
	/// Assigns a ghost role to a player, despawns their ghost, and removes the role from availability.
	/// </summary>
	/// <param name="RoleId">The index of the role to assign</param>
	/// <param name="PlayerId">The multiplayer ID of the player receiving the role</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void GiveGhostRole(int RoleId, int PlayerId)
	{
		if (Multiplayer.IsServer())
		{
			RoleNodes[RoleId].Rpc("ChangeOwner", PlayerId);
			DespawnGhost(PlayerId);
			Rpc("RemoveGhostRole", RoleId);
		}
		else
		{
			RpcId(1, "GiveGhostRole", RoleId, PlayerId);
		}
	}

	/// <summary>
	/// Synchronizes all currently available ghost roles to a specific client.
	/// </summary>
	/// <param name="Id">The multiplayer ID of the target client</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SyncGhostRoles(int Id)
	{
		if (Multiplayer.IsServer())
		{
			foreach (GhostRoleData item in GhostRoles)
			{
				RpcId(Id, "LocalAddGhostRole", item.RoleName, item.RoleDesc);
			}
		}
		else
		{
			RpcId(1, "SyncGhostRoles", Id);
		}
	}

	/// <summary>
	/// Locally adds a ghost role to the roles list on this client. Called via RPC from the server.
	/// </summary>
	/// <param name="Name">The name of the ghost role</param>
	/// <param name="Desc">The description of the ghost role</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void LocalAddGhostRole(string Name, string Desc)
	{
		GhostRoles.Add(new GhostRoleData()
		{
			RoleName = Name,
			RoleDesc = Desc
		});
	}

	/// <summary>
	/// Locally removes a ghost role from the roles list on this client with bounds checking. Called via RPC from the server.
	/// </summary>
	/// <param name="RoleId">The index of the role to remove</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void LocalRemoveGhostRole(int RoleId)
	{
		if (RoleId < 0 || RoleId >= GhostRoles.Count)
		{
			GD.PushError($"LocalRemoveGhostRole: invalid RoleId {RoleId}, GhostRoles count = {GhostRoles.Count}");
			return;
		}
		GhostRoles.RemoveAt(RoleId);
	}

	/// <summary>
	/// Spawns a ghost character for the calling player across the network.
	/// </summary>
	public void SpawnGhost()
	{
		Rpc("RpcSpawnGhost", Multiplayer.GetUniqueId());
	}

	/// <summary>
	/// Despawns a ghost character from the network.
	/// </summary>
	/// <param name="GhostPlayerId">The multiplayer ID of the ghost player to despawn</param>
	public void DespawnGhost(int GhostPlayerId)
	{
		Rpc("RpcDespawnGhost", GhostPlayerId);
	}

	/// <summary>
	/// RPC method that instantiates a ghost on all clients and sets the ghost's multiplayer authority.
	/// </summary>
	/// <param name="GhostPlayerId">The multiplayer ID of the player who owns this ghost</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcSpawnGhost(int GhostPlayerId)
	{
		var GhostInstance = GhostScene.Instantiate();
		GhostInstance.SetMultiplayerAuthority(GhostPlayerId);
		GetNode<Node>("..").GetChildren().OfType<Node3D>().FirstOrDefault().AddChild(GhostInstance);
		GhostInstance.Name = GhostInstance.Name + GhostPlayerId;
	}

	/// <summary>
	/// RPC method that removes a ghost character from the game world by finding and freeing it by its multiplayer authority.
	/// </summary>
	/// <param name="GhostPlayerId">The multiplayer ID of the ghost player to despawn</param>
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
