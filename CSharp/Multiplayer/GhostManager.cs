//Licensed under AGPL 3.0. Glory to communism!
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GhostManager : Node
{
	public List<GhostRoleData> GhostRoles = new List<GhostRoleData>();
	private List<Node3D> RoleNodes = new List<Node3D>();

	[Export] private PackedScene GhostScene;

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void AddGhostRole(string Name, string Desc, string Path)
	{
		if (Multiplayer.IsServer())
		{
			Rpc("LocalAddGhostRole", Name, Desc);
			RoleNodes.Add(GetNode<Node3D>(Path));
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RemoveGhostRole(int RoleId)
	{
		if (Multiplayer.IsServer())
		{
			Rpc("LocalRemoveGhostRole", RoleId);
			RoleNodes.RemoveAt(RoleId);
		}
	}

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

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void LocalAddGhostRole(string Name, string Desc)
	{
		GhostRoles.Add(new GhostRoleData()
		{
			RoleName = Name,
			RoleDesc = Desc
		});
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	void LocalRemoveGhostRole(int RoleId)
	{
		if (RoleId < 0 || RoleId >= GhostRoles.Count)
		{
			GD.PushError($"LocalRemoveGhostRole: invalid RoleId {RoleId}, GhostRoles count = {GhostRoles.Count}");
			return;
		}
		GhostRoles.RemoveAt(RoleId);
	}

	public void PrintGhostRoles()
	{
		GD.Print("Current Ghost Roles:");
		foreach (GhostRoleData item in GhostRoles)
		{
			GD.Print(item.RoleName);
		}
		GD.Print("GhostRolePrint ended");
	}

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
