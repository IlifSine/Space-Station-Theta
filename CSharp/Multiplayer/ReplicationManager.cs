//Licensed under AGPL 3.0
using Godot;

public partial class ReplicationManager : Node
{
	private BasicMultiplayerManager BMM;
	private string BMMPath = "/root/BasicMultiplayerManager";
	private string MapScenePath = "res://Scenes/World/GameMap.tscn";
	private string GameWorldPath = "/root/GameWorld";

	public override void _Ready()
	{
		BMM = GetNode<BasicMultiplayerManager>(BMMPath);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void GetAll(long Id)
	{
		if (Multiplayer.IsServer())
		{
			foreach (var MapItem in GetNode<GameWorld>(GameWorldPath).GetChildren())
			{
				if (MapItem is GameMap)
				{
					foreach (var ObjectItem in GetNode<GameMap>(GameWorldPath + "/" + MapItem.Name).GetChildren())
					{
						string ObjectPath = ObjectItem.SceneFilePath;
						Vector3 ObjectPosition;
						if (ObjectItem is Node3D Object3d)
						{
							ObjectPosition = Object3d.Position;
						}
						else
						{
							ObjectPosition = new Vector3();
						}
						RpcId(Id, "ReplicateObject", ObjectPath, MapItem.Name, ObjectItem.Name, ObjectPosition);
					}
				}
			}
		}
		else
		{
			RpcId(1, "GetAll", Id);
		}
	}

	public void ReplicateMap(GameMap Map)
	{
		foreach (var ObjectItem in GetNode<GameMap>(GameWorldPath + "/" + Map.Name).GetChildren())
		{
			string ObjectPath = ObjectItem.SceneFilePath;
			Vector3 ObjectPosition;
			if (ObjectItem is Node3D Object3d)
			{
				ObjectPosition = Object3d.Position;
			}
			else
			{
				ObjectPosition = new Vector3();
			}
			Rpc("ReplicateObject", ObjectPath, Map.Name, ObjectItem.Name, ObjectPosition);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReplicateObject(string ObjectPath, string MapName, string ObjectName, Vector3 ObjectPosition)
	{
		PackedScene LoadedObjectScene = new PackedScene();
		if (ObjectPath != "")
		{
			LoadedObjectScene = ResourceLoader.Load<PackedScene>(ObjectPath);
		}
		else
		{
			LoadedObjectScene.Pack(new CsgBox3D());
		}
		var InstantiatedObject = LoadedObjectScene.Instantiate();

		var ObjectMap = GetNodeOrNull<Node3D>(GameWorldPath + "/" + MapName);
		if (ObjectMap != null)
		{
			ObjectMap.AddChild(InstantiatedObject);
		}
		else
		{
			var PreLoadedMapScene = ResourceLoader.Load<PackedScene>(MapScenePath);
			ObjectMap = PreLoadedMapScene.Instantiate() as GameMap;
			GetNode<Node>(GameWorldPath).AddChild(ObjectMap);
			ObjectMap.AddChild(InstantiatedObject);
		}

		InstantiatedObject.Name = ObjectName;
		if (InstantiatedObject is Node3D InstantiatedObject3d)
		{
			InstantiatedObject3d.Position = ObjectPosition;
		}
	}
}
