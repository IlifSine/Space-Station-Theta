//Licensed under AGPL 3.0
using Godot;

public partial class GameWorld : Node
{
	public void LoadMap(string Map)
	{
		GameMap LoadMap = null;
		switch (Map)
		{
			case "Dev":
				LoadMap = ResourceLoader.Load<PackedScene>("res://Maps/GameMapDev.tscn").Instantiate<GameMap>();
			break;
			default:
				GD.Print("Map not found!");
			break;
		}
		if (LoadMap != null)
		{
			AddChild(LoadMap);
		}
	}

	public void LoadMapFromPath(string MapPath)
	{
		GameMap LoadMap;
		LoadMap = ResourceLoader.Load<PackedScene>(MapPath).Instantiate<GameMap>();
		if (LoadMap != null)
		{
			AddChild(LoadMap);
		}
	}
}
