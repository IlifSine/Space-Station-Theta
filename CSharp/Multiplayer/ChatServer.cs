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

	public void ReceiveSentMessage(string Message, string SenderName)
	{
		RpcId(1, "SendMessage", Message, SenderName);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SendMessage(string Message, string SenderName)
	{
		//Replacing BBCode-used symbols to players cant use bbcode. muhahahahahahah
		Message = Message.Replace("[", "");
		Message = Message.Replace("]", "");
		Message = string.Format("{0}: {1}", SenderName, Message);
		foreach (ChatPanel item in ChatClients)
		{
			item.MessageReceive(Message);
		}
	}
}
