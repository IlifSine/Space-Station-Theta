//Licensed under AGPL 3.0. Glory to communism!
using System.Collections.Generic;
using Godot;

public partial class ChatServer : Node
{
	public List<ChatPanel> ChatClients = new List<ChatPanel>();

	public void AddClient(Node ControlChatPanel)
	{
		if (Multiplayer.IsServer())
		{
			if (ControlChatPanel is ChatPanel chatPanel)
			{
				ChatClients.Add(chatPanel);
			}
		}
	}

	public void ReceiveSentMessage(string Message)
	{
		RpcId(1, "SendMessage", Message);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SendMessage(string Message)
	{
		GD.Print(ChatClients.Count);
		foreach (ChatPanel item in ChatClients)
		{
			item.MessageReceive(Message);
		}
	}
}
