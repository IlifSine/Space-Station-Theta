using Godot;

public partial class ParentVisibleButton : Button
{
	Control ParentNode;

	public override void _Ready()
	{
		ParentNode = GetNode<Control>("../");
	}

	void OnPressed()
	{
		ParentNode.Visible = false;
	}
}
