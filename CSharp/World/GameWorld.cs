//Licensed under AGPL 3.0
using Godot;

public partial class GameWorld : Node
{
	private BasicMultiplayerManager BMM;
	private string BMMPath = "/root/BasicMultiplayerManager";

	public override void _Ready()
	{
		BMM = GetNode<BasicMultiplayerManager>(BMMPath);
	}

	//public override void _Process(double delta)
	//{
	//    foreach (var item in BMM.ConnectedPlayersData)
	//    {
	//        foreach (var ObjectItem in GetChildren())
	//        {
	//            if (ObjectItem.Dis)
	//        }
	//    }
	//}
}
