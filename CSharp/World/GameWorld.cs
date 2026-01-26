//Licensed under AGPL 3.0
using Godot;

public partial class GameWorld : Node
{
	public void LoadMap(string Map)
	{
		GD.Print("e");
		Node3D LoadMap = null;
		LoadMap = ResourceLoader.Load<PackedScene>("res://Scenes/World/GameMapDev.tscn").Instantiate<Node3D>();
		AddChild(LoadMap);
		/*switch (Map)
		{
			case "Dev":
				LoadMap = ResourceLoader.Load<PackedScene>("res://Scenes/World/GameMap.tscn").Instantiate<Node3D>();
				AddChild(LoadMap);
				GD.Print("l");
			break;
			default:
				LoadMap = ResourceLoader.Load<PackedScene>(Map).Instantiate<Node3D>();
				AddChild(LoadMap);
				GD.Print("АЛАРМА АЛАРМА АЛАМО АЛДЫБАХА");
			break;
		}
		GD.Print("яяя");
		*/
	}
}
