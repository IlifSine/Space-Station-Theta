//Licensed under AGPL 3.0
using Godot;

public partial class ChatPanel : Panel
{
	[Export] private LineEdit lineEdit;
	[Export] private RichTextLabel richTextLabel;
	private string ChatServerPath = "/root/ChatServer";
	private ChatServer chatServer;

	public override void _Ready()
	{
		chatServer = GetNode<ChatServer>(ChatServerPath);
		if (Multiplayer.IsServer())
		{
			chatServer.AddClient(this);
		}
	}

	private void MessageEnter(string Message)
	{
		chatServer.ReceiveSentMessage(Message);
		lineEdit.Clear();
	}

	public void MessageReceive(string Message)
	{
		Rpc("MessageReceiveRpc", Message);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void MessageReceiveRpc(string Message)
	{
		richTextLabel.Text = string.Format("{0}\n{1}", richTextLabel.Text, Message);
	}
}