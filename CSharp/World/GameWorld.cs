//Licensed under AGPL 3.0. Glory to communism!
using System.Collections.Generic;
using Godot;

public partial class GameWorld : Node
{
	public List<GhostRoleData> GhostRoles = new List<GhostRoleData>();
	List<Node3D> RoleNodes = new List<Node3D>();
	string ReplicationManagerPath = "/root/ReplicationManager";
	ReplicationManager replicationManager;

	public override void _Ready()
	{
		replicationManager = GetNode<ReplicationManager>(ReplicationManagerPath);
	}

	//Ghost roles

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void AddGhostRole(string Name, string Desc, string Path)
	{
		if (Multiplayer.IsServer())
		{
			Rpc("LocalAddGhostRole", Name, Desc);
			RoleNodes.Add(GetNode<Node3D>(Path));
		}
		/*else
		{
			RpcId(1, "AddGhostRole", Name, Desc);
		}*/
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RemoveGhostRole(int RoleId)
	{
		if (Multiplayer.IsServer())
		{
			Rpc("LocalRemoveGhostRole", RoleId);
			RoleNodes.RemoveAt(RoleId);
		}
		/*else
		{
			RpcId(1, "RemoveGhostRole", RoleId);
		}*/
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void GiveGhostRole(int RoleId, int PlayerId)
	{
		if (Multiplayer.IsServer())
		{
			RoleNodes[RoleId].SetMultiplayerAuthority(PlayerId);
			RoleNodes[RoleId].Rpc("RefreshAuthority");
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

	//Local ghost role methods

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

	//Debug ghost role methods

	public void PrintGhostRoles()
	{
		GD.Print("Current Ghost Roles:");
		foreach (GhostRoleData item in GhostRoles)
		{
			GD.Print(item.RoleName);
		}
		GD.Print("GhostRolePrint ended");
	}

	/// <summary>
	/// LoadMapRequest method requests server and clients to load map.
	/// </summary>
	/// <param name="Map"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void LoadMap(string Map)
	{
		if (Multiplayer.IsServer())
		{
			Rpc("InstantiateMap", Map);
		}
		else
		{
			RpcId(1, "LoadMap", Map);
		}
	}

	/// <summary>
	/// LoadMap method loads a map from some kind of "database" located in it by name. If method couldn't find a map with this name - it pushes error.
	/// </summary>
	/// <param name="Map"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void InstantiateMap(string Map)
	{
		GameMap LoadMap = null;
		switch (Map)
		{
			case "Dev":
				LoadMap = ResourceLoader.Load<PackedScene>("res://Maps/GameMapDev.tscn").Instantiate<GameMap>();
			break;
			default:
				GD.PushError("No map found.");
			break;
		}
		if (LoadMap != null)
		{
			AddChild(LoadMap);
			LoadMap.Name = LoadMap.DefaultName;
		}
	}

	/// <summary>
	/// LoadMapFromPath method loads map from string res:// path. If method couldn't find a map by this path - it pushes error.
	/// </summary>
	/// <param name="MapPath"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void LoadMapFromPath(string MapPath)
	{
		if (Multiplayer.IsServer())
		{
			GameMap LoadMap;
			LoadMap = ResourceLoader.Load<PackedScene>(MapPath).InstantiateOrNull<GameMap>();
			if (LoadMap != null)
			{
				AddChild(LoadMap);
				LoadMap.Name = LoadMap.DefaultName;
			}
			else
			{
				GD.PushError("No map found.");
			}	
			replicationManager.ReplicateMap(LoadMap);
		}
		else
		{
			RpcId(1, "LoadMapFromPath", MapPath);
		}
	}
}
