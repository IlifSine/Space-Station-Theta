using Godot;

public partial class MapLoaderMenu : Control
{
	[Export] private LineEdit lineEdit;
	private GameWorld gameWorld;

	public override void _Ready()
	{
		gameWorld = GetNode<GameWorld>("/root/GameWorld");
	}

	public void LoadMapButtonPressed()
	{
		gameWorld.LoadMap(lineEdit.Text);
	}

	public void LoadMapPathButtonPressed()
	{
		gameWorld.LoadMapFromPath(lineEdit.Text);
	}
}
