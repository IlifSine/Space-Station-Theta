using Godot;

public partial class VisibleButton : Button
{
	[Export] public Control ChangeControl;

	void OnPressed()
	{
		ChangeControl.Visible = !ChangeControl.Visible;
	}
}
