//open-source EULA/CLA, see full text in LICENSE.txt
using System;
using System.Linq;
using Godot;

public partial class MultiplayerController : Control
{
	private int port = 8910;
	private string address;

	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		if (OS.GetCmdlineArgs().Contains("--server"))
		{
			HostGame();
		}
	}

	private void HostGame()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var Error = peer.CreateServer(port, 2);
		if (Error != Error.Ok)
		{
			GD.Print("Host error:" + Error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Server host succesful");
	}

	private void ConnectionFailed()
	{
		GD.Print("Connection failed");
	}

	private void ConnectedToServer()
	{
		GD.Print("Connected to server");
		RpcId(1, "SendPlayerInformation", GetNode<LineEdit>("PlayerNameLine").Text, Multiplayer.GetUniqueId());
		
		//this awful thing is not working. ahhhhh
		//foreach (var item in GameManager.Players)
		//{
		//	GD.Print(item.Name + " is playing");
		//}
		//var GameScene = ResourceLoader.Load<PackedScene>("res://Scenes/GameScene.tscn").Instantiate<Node3D>();
		//GetTree().Root.AddChild(GameScene);
		//this.Hide();
	}

	private void PeerDisconnected(long id)
	{
		GD.Print("Player with id " + id.ToString() + " disconnected");
		GameManager.Players.Remove(GameManager.Players.Where(i => i.Id == id).First<PlayerData>());
		var Players = GetTree().GetNodesInGroup("Player");	
		foreach (var item in Players)
		{
			if (item.Name == id.ToString())
			{
				item.QueueFree();
			}
		}
	}

	private void PeerConnected(long id)
	{
		GD.Print("Player with id " + id.ToString() + " connected");
	}

	private void JoinGamePresssed()
	{
		address = GetNode<LineEdit>("ServerIPLine").Text;
		//idk why but its not working, can anyone fix please?
		//port = GetNode<LineEdit>("ServerPortLine").Text.ToInt();
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		peer.CreateClient(address, port);

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining game");
	}

	private void HostGamePresssed()
	{
		HostGame();
		SendPlayerInformation(GetNode<LineEdit>("PlayerNameLine").Text, 1);
	}

	private void StartGamePresssed()
	{
		Rpc("StartGame");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame()
	{
		foreach (var item in GameManager.Players)
		{
			GD.Print(item.Name + " is playing");
		}
		var GameScene = ResourceLoader.Load<PackedScene>("res://Scenes/Main/GameScene.tscn").Instantiate<Node3D>();
		GetTree().Root.AddChild(GameScene);
		this.Hide();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void SendPlayerInformation(string name, int id)
	{
		PlayerData playerData = new PlayerData()
		{
			Name = name,
			Id = id
		};
		if (!GameManager.Players.Contains(playerData))
		{
			GameManager.Players.Add(playerData);
		}
		if (Multiplayer.IsServer())
		{
			foreach (var item in GameManager.Players)
			{
				Rpc("SendPlayerInformation", item.Name, item.Id);
			}
		}
	}
}
