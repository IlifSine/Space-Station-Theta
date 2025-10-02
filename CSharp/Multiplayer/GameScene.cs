//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;

public partial class GameScene : Node3D
{
	[Export] private PackedScene PlayerScene;

	public override void _Ready()
	{
		int index = 0;
		foreach (var Item in GameManager.Players)
		{
			BiogicalKineticalHumanoid CurrentPlayer = PlayerScene.Instantiate<BiogicalKineticalHumanoid>();
			CurrentPlayer.Name = Item.Id.ToString();
			AddChild(CurrentPlayer);
			foreach (Node3D SpawnPoint in GetTree().GetNodesInGroup("PlayerSpawnPoints"))
			{
				if (int.Parse(SpawnPoint.Name) == index)
				{
					CurrentPlayer.GlobalPosition = SpawnPoint.GlobalPosition;
				}
			}
			index++;
		}
	}

	public void SendChatMessage(string Message)
	{
		RpcId(1, "ServerSendChatMessage", Message);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ServerSendChatMessage(string Message)
	{
		foreach (var Item in GameManager.Players)
		{
			long CurrentCallID = Item.Id;
			RpcId(CurrentCallID, "GetChatMessage", Message);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void GetChatMessage(string Message)
	{
		CharacterBody3D PlayerBody = GetNode<CharacterBody3D>(Multiplayer.GetUniqueId().ToString());
		PlayerBody.Call("GetChatMessage", Message);
	}
}
