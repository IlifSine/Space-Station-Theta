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
		chatServer.AddClient(this);
	}

	private void MessageEnter()
	{
		chatServer.ReceiveSentMessage(lineEdit.Text);
		lineEdit.Clear();
	}

	public void MessageReceive(string Message)
	{
		Rpc("MessageReciveRpc");
	}

	private void MessageReceiveRpc(string Message)
	{
		richTextLabel.Text = richTextLabel.Text + "/n" + Message;
	}
}
