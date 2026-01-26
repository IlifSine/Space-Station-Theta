//Licensed under AGPL 3.0
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class BasicMultiplayerManager : Node
{
	private string LobbyMenuPath = "res://Scenes/Menu/LobbyMenu.tscn";
	private string ServerPanelPath = "res://Scenes/Menu/LobbyMenu.tscn";

	private string GameWorldPath = "/root/GameWorld";
	private GameWorld GameWorldInstance;
	private string ReplicationManagerPath = "/root/ReplicationManager";
	private ReplicationManager ReplicationManagerInstance;

	private string SelfCkey = "Player";
	private int HostPort = 8910;

	public List<PlayerData> ConnectedPlayersData = new List<PlayerData>();

	public override void _Ready()
	{
		ReplicationManagerInstance = GetNode<ReplicationManager>(ReplicationManagerPath);
		GameWorldInstance = GetNode<GameWorld>(GameWorldPath);

		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;

		if (OS.GetCmdlineArgs().Contains("--server"))
		{
			HostGame();
		}
	}

	//Method needed to host server
	public void HostGame()
	{
		//Estabilishing connection
		ENetMultiplayerPeer Peer = new ENetMultiplayerPeer();
		var Error = Peer.CreateServer(HostPort, 32);
		if (Error != Error.Ok)
		{
			GD.Print("Host error:" + Error.ToString());
			return;
		}
		Peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = Peer;

		//Deleting main menu
		var MM = GetTree().Root.GetChildren().OfType<MainMenu>().FirstOrDefault();
		MM.QueueFree();
		//Loading server panel
		/*var ServerPanelInstance = ResourceLoader.Load<PackedScene>(ServerPanelPath).Instantiate();
		GetTree().Root.AddChild(ServerPanelInstance);*/
		//Loading map
		GD.Print("a");
		//GameWorldInstance.LoadMap("Dev");
		Node3D LoadMap;
		LoadMap = ResourceLoader.Load<PackedScene>("res://Scenes/World/GameMapDev.tscn").Instantiate<Node3D>();
		GameWorldInstance.AddChild(LoadMap);

		GD.Print("Hosted server");     
	}

	//This method connects player to server
	public void JoinGame(string Address, int Port)
	{
		GD.Print("Connecting to server");
		//Estabilishing connection
		ENetMultiplayerPeer Peer = new ENetMultiplayerPeer();
		Peer.CreateClient(Address, Port);
		Peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = Peer;
	}

	private void ConnectedToServer()
	{
		RpcId(1, "AddConnectedPlayer", SelfCkey, Multiplayer.GetUniqueId());
		GD.Print("Connected to server");

		//Deleting main menu
		var MM = GetTree().Root.GetChildren().OfType<MainMenu>().FirstOrDefault();
		MM.QueueFree();
		//Loading game objects with ReplicationManager
		ReplicationManagerInstance.GetObjects(Multiplayer.GetUniqueId());
		//Loading lobby
		var LobbyMenuInstance = ResourceLoader.Load<PackedScene>(LobbyMenuPath).Instantiate<LobbyMenu>();
		GetTree().Root.AddChild(LobbyMenuInstance);
	}

	private void ConnectionFailed()
	{
		GD.Print("Connection failed");
	}

	//This method happens when someone connects to server
	private void PeerConnected(long ConnectedId)
	{
		GD.Print("Player {0} connected", ConnectedId);
	}

	//This method happens when someone disconnects from server
	private void PeerDisconnected(long DisconnectedId)
	{
		GD.Print("Player {0} disconnected", DisconnectedId);
		RemoveConnectedPlayer(DisconnectedId);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void AddConnectedPlayer(string ConnectedCkey, long ConnectedId)
	{
		bool IsExist = false;
		PlayerData AddedPlayerData = new PlayerData()
		{
			Ckey = ConnectedCkey,
			Id = ConnectedId
		};

		foreach (PlayerData item in ConnectedPlayersData)
		{
			if (item.Id == ConnectedId)
			{
				IsExist = true;
				break;
			}
		}

		if (!IsExist) 
		{
			ConnectedPlayersData.Add(AddedPlayerData);
		}

		if (Multiplayer.IsServer())
		{
			foreach (PlayerData item in ConnectedPlayersData)
			{
				Rpc("AddConnectedPlayer", item.Ckey, item.Id);
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void RemoveConnectedPlayer(long DisconnectedId)
	{
		foreach (PlayerData item in ConnectedPlayersData)
		{
			if (item.Id == DisconnectedId)
			{
				ConnectedPlayersData.Remove(item);
			}
		}

		if (Multiplayer.IsServer())
		{
			foreach (PlayerData item in ConnectedPlayersData)
			{
				Rpc("RemoveConnectedPlayer", item.Id);
			}
		}
	}
}
