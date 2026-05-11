//Licensed under AGPL 3.0. Glory to communism!
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
		//Deleting old GhostRoleEntrys
		foreach (var item in Container.GetChildren())
		{
			item.QueueFree();
		}
		//Instantiateing new GhostRoleEntrys
		int roleIndex = 0;
		foreach (GhostRoleData item in gameWorld.GhostRoles)
		{
			GhostRoleEntry EntryInstance = PackedEntry.Instantiate<GhostRoleEntry>();
			EntryInstance.RoleNameLabel.Text = item.RoleName;
			EntryInstance.RoleDescLabel.Text = item.RoleDesc;
			EntryInstance.RoleIndex = roleIndex;
			Container.AddChild(EntryInstance);
			roleIndex++;
		}
	}

	public void PickRole(int RoleIndex)
	{
		gameWorld.GiveGhostRole(RoleIndex, Multiplayer.GetUniqueId());
	}
}
