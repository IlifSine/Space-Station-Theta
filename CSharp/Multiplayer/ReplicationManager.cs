//Licensed under AGPL 3.0
using System;
using Godot;

public partial class ReplicationManager : Node
{
	private BasicMultiplayerManager BMM;
	private string BMMPath = "/root/BasicMultiplayerManager";
	private string GameMapPath = "/root/GameWorld/GameMap";
	private bool Authority;

	public override void _Ready()
	{
		BMM = GetNode<BasicMultiplayerManager>(BMMPath);
		Authority = Multiplayer.GetUniqueId() == 1;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void GetObjects(long Id)
	{
		if (Authority)
		{
			foreach (var ObjectItem in GetNode<Node3D>(GameMapPath).GetChildren())
			{
				var PackedObjectScene = new PackedScene();
				PackedObjectScene.Pack(ObjectItem);
				ReplicationData ObjectData = new ReplicationData()
				{
					ObjectType = ObjectItem.GetType(),
					ObjectScene = PackedObjectScene,
					ObjectTransform = new Transform3D()
				};
				if (ObjectItem is Node3D Node3DItem)
				{
					ObjectData.ObjectTransform = Node3DItem.Transform;
				}
				RpcId(Id, "ReplicateObject", ObjectData);
			}
		}
		else
		{
			RpcId(1, "GetObjects", Id);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReplicateObject(ReplicationData ObjectData)
	{
		Type ObjectType = ObjectData.ObjectType;
		GD.Print(ObjectType);
		GD.Print(ObjectData.ObjectTransform);
		ObjectType InstantiatedObject = ObjectData.ObjectScene.Instantiate() as ObjectType;
	}
}
