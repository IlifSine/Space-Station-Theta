//Licensed under AGPL 3.0
using Godot;

public partial class GameWorld : Node
{
	private BasicMultiplayerManager BMM;
	private string BMMPath = "/root/BasicMultiplayerManager";
	private bool Authority;

	public override void _Ready()
	{
		BMM = GetNode<BasicMultiplayerManager>(BMMPath);
		Authority = Multiplayer.GetUniqueId() == GetMultiplayerAuthority();
	}

	public void GetObjects()
	{
		GD.Print("eh");
		if (Authority)
		{
			GD.Print("uh");
			foreach (var item in BMM.ConnectedPlayersData)
			{
				foreach (var ObjectItem in GetChildren())
				{
					Rpc("ReplicateObject", ObjectItem);
				}
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ReplicateObject(Variant ReplicatingObject)
	{
		GD.Print("replicated?");
	}
}
