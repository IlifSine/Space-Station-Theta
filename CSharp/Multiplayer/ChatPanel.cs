using Godot;

public partial class ChatPanel : Panel
{
	[Export] private LineEdit lineEdit;
	[Export] private RichTextLabel richTextLabel;
	private string ChatServerPath = "/root/ChatServer";
	private ChatServer chatServer;

	public void _Ready()
	{
		chatServer = GetNode<ChatServer>(ChatServerPath);
		chatServer.
	}

	private void MessageEnter()
	{
		chatServer.SendMessage(lineEdit.Text);
	}
}
