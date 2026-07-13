//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class GhostRoleEntry : Button
{
	[Export] public Label RoleNameLabel;
	[Export] public Label RoleDescLabel;
	public int RoleIndex;

	private void PickRole()
	{
		GetNode<GhostRolePanel>("../../").PickRole(RoleIndex);
		QueueFree();
	}
}
