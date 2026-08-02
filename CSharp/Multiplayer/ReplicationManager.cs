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

	/// <summary>
	/// This method gets all maps from server and replicates it (calls GetAllServer). Called from client.
	/// </summary>
	/// <param name="Id"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void GetAll(long Id)
	{
		if (Id != 1)
		{
			RpcId(1, MethodName.GetAllServer, Id);
		}
	}

	/// <summary>
	/// Server-only method that actually gets and sends all maps.
	/// </summary>
	/// <param name="Id"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void GetAllServer(long Id)
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
			}
		}
		else
		{
			RpcId(1, MethodName.ReplicateMap, Map);
		}
	}

	/// <summary>
	/// Instantiates 1 object on client with these params.
	/// </summary>
	/// <param name="ObjectPath"></param>
	/// <param name="MapName"></param>
	/// <param name="ObjectName"></param>
	/// <param name="ObjectPosition"></param>
	/// <param name="ObjectRotation"></param>
	/// <param name="ObjectAuthority"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
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
