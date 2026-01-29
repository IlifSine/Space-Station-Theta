using System.Collections.Generic;
using Godot;

public partial class ChatServer : Node
{
	public List<ChatPanel> ChatClients = new List<ChatPanel>();

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void AddClient(ChatPanel chatPanel)
	{
		if (Multiplayer.IsServer())
		{
			ChatClients.Add(chatPanel);
		}
		else
		{
			RpcId(1, "AddClient", chatPanel);
		}
	}

	public void ReceiveSentMessage(string Message)
	{
		RpcId(1, "SendMessageServer", Message);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SendMessage(string Message)
	{
		foreach (ChatPanel item in ChatClients)
		{
			item.MessageReceive(Message);
		}
	}
}