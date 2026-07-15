//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class ReplicationManager : Node
{
	private BasicMultiplayerManager BMM;
	[Export] private PackedScene MapScenePath;
	private string BMMPath = "/root/BasicMultiplayerManager";
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
						Vector3 ObjectRotation;
						if (ObjectItem is Node3D Object3d)
						{
							ObjectPosition = Object3d.Position;
							ObjectRotation = Object3d.Rotation;
						}
						else
						{
							ObjectPosition = new Vector3();
							ObjectRotation = new Vector3();
						}

						RpcId(Id, MethodName.ReplicateObject, ObjectPath, MapItem.Name, ObjectItem.Name, ObjectPosition, ObjectRotation, ObjectItem.GetMultiplayerAuthority());
					}
				}
			}
		}
		else
		{
			RpcId(1, MethodName.GetAll, Id);
		}
	}

	/// <summary>
	/// Replicates already instanced map trough all clients. Only-server method.
	/// </summary>
	/// <param name="Map">Instanced map</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReplicateMap(GameMap Map)
	{
		if (Multiplayer.IsServer())
		{
			foreach (var ObjectItem in GetNode<GameMap>(GameWorldPath + "/" + Map.Name).GetChildren())
			{
				string ObjectPath = ObjectItem.SceneFilePath;
				Vector3 ObjectPosition;
				Vector3 ObjectRotation;
				if (ObjectItem is Node3D Object3d)
				{
					ObjectPosition = Object3d.Position;
					ObjectRotation = Object3d.Rotation;
				}
				else
				{
					ObjectPosition = new Vector3();
					ObjectRotation = new Vector3();
				}

				Rpc(MethodName.ReplicateObject, ObjectPath, Map.Name, ObjectItem.Name, ObjectPosition, ObjectRotation, ObjectItem.GetMultiplayerAuthority());

				//Objects was duplicating on 1 client, so i decided to QueueFree() original object. Yes, i so lazy to find normal soulution.
				ObjectItem.QueueFree();
			}
		}
		else
		{
			RpcId(1, MethodName.ReplicateMap, Map);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	public void ReplicateObject(string ObjectPath, string MapName, string ObjectName, Vector3 ObjectPosition, Vector3 ObjectRotation, int ObjectAuthority)
	{
		PackedScene LoadedObjectScene = new PackedScene();
		if (ObjectPath != "")
		{
			LoadedObjectScene = ResourceLoader.Load<PackedScene>(ObjectPath);
		}
		else
		{
			LoadedObjectScene.Pack(new Node());
		}
		var InstantiatedObject = LoadedObjectScene.Instantiate();

		var ObjectMap = GetNodeOrNull<Node3D>(GameWorldPath + "/" + MapName);
		if (ObjectMap != null)
		{
			ObjectMap.AddChild(InstantiatedObject);
		}
		else
		{
			var PreLoadedMapScene = MapScenePath;
			ObjectMap = PreLoadedMapScene.Instantiate() as GameMap;
			GetNode<Node>(GameWorldPath).AddChild(ObjectMap);
			ObjectMap.AddChild(InstantiatedObject);
			ObjectMap.Name = MapName;
		}

		InstantiatedObject.Name = ObjectName;
		if (InstantiatedObject is Node3D InstantiatedObject3d)
		{
			InstantiatedObject3d.Position = ObjectPosition;
			InstantiatedObject3d.Rotation = ObjectRotation;
		}

		if (ObjectAuthority != 1)
		{
			InstantiatedObject.SetMultiplayerAuthority(ObjectAuthority);
		}
	}
}
