//Licensed under AGPL 3.0
using Godot;

public partial class PlayerList : ItemList
{
	private BasicMultiplayerManager BMM;

	public override void _Ready()
	{
		BMM = GetNode<BasicMultiplayerManager>("/root/BasicMultiplayerManager");
	}

	public void RefreshButtonPressed()
	{
		Clear();
		foreach (var item in BMM.ConnectedPlayersData)
		{
			AddItem(item.Ckey + item.Id);
		}
	}
}
