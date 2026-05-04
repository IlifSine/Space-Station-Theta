using Godot;

public partial class GhostRolePanel : Panel
{
	[Export] VBoxContainer Container;
	[Export] PackedScene PackedEntry;
	string GameWorldPath = "/root/GameWorld";
	GameWorld gameWorld = new GameWorld();
	public override void _Ready()
	{
		gameWorld = GetNode<GameWorld>(GameWorldPath);
	}

	public void RefreshRoles()
	{   
		foreach (GhostRoleData item in gameWorld.GhostRoles)
		{
			var EntryInstance = PackedEntry.Instantiate();
			EntryInstance.GetNode<Label>("RoleNameLabel").Text = item.RoleName;
			EntryInstance.GetNode<Label>("RoleDescLabel").Text = item.RoleDesc;
			Container.AddChild(EntryInstance);
		}
		gameWorld.PrintGhostRoles();
	}
}
