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
		RpcId(1, MethodName.SendMessage, Message, SenderName);
	}

	/// <summary>
	/// Send a message to all connected chat clients
	/// </summary>
	/// <param name="Message"></param>
	/// <param name="SenderName"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void SendMessage(string Message, string SenderName)
	{
		Message = $"{SenderName}: {Message.Sanitize()}";
		foreach (ChatPanel item in ChatClients)
		{
			item.MessageReceive(Message);
		}
	}
}
