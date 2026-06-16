//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class VisibleButton : Button
{
	[Export] public Control ChangeControl;

	void OnPressed()
	{
		ChangeControl.Visible = !ChangeControl.Visible;
	}
}
