//Licensed under AGPL 3.0. Glory to communism!
using Godot;
using System;

public partial class AdvancedCharacter : BasicCharacter
{
	[Export] private RayCast3D InteractionRay;
	[Export] private Label RightHandPanelLabel;
	[Export] private Label LeftHandPanelLabel;
	[Export] private Node3D RightHand;
	[Export] private Node3D LeftHand;
	private Item RightHandItem;
	private Item LeftHandItem;
	private int HandSelected = 0; //0 - Right hand, 1 - Left. 2+ - more arahnid and other monsters' hands.
	private const int HandCount = 1; //Count from 0!!! 

	/*public override void _Input(InputEvent Event)
	{
		if (Event.IsActionPressed("LMB"))
		{
			Rpc(MethodName.LMB);
		}
		if (Event.IsActionPressed("DropItem"))
		{
			Rpc(MethodName.DropItem);
		}
		if (Event.IsActionPressed("ChangeHand"))
		{
			Rpc(MethodName.ChangeHand);
		}
	}*/

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void LMB()
	{
		Item CurrentHandItem;
		Node3D CurrentHandNode;
		Label CurrentPanelLabel;
		switch (HandSelected)
		{	
			case 0:
				CurrentHandItem = RightHandItem;
				CurrentHandNode = RightHand;
				CurrentPanelLabel = RightHandPanelLabel;
				break;
			case 1:
				CurrentHandItem = LeftHandItem;
				CurrentHandNode = LeftHand;
				CurrentPanelLabel = LeftHandPanelLabel;
				break;
			default:
				CurrentHandItem = RightHandItem;
				CurrentHandNode = RightHand;
				CurrentPanelLabel = RightHandPanelLabel;
				break;
		}

		if (CurrentHandItem == null)
		{
			if (InteractionRay.GetCollider() is Item PickingItem)
			{
				PickingItem.GetParent().RemoveChild(PickingItem);
				CurrentHandNode.AddChild(PickingItem);
				PickingItem.Position = Vector3.Zero;
				
				CurrentPanelLabel.Text = PickingItem.Name;

				CurrentHandItem = PickingItem;
			}
		}

		switch (HandSelected)
		{	
			case 0:
				RightHandItem = CurrentHandItem;
				break;
			case 1:
				LeftHandItem = CurrentHandItem;
				break;
			default:
				RightHandItem = CurrentHandItem;
				break;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void DropItem()
	{
		Item CurrentHandItem;
		Node3D CurrentHandNode;
		Label CurrentPanelLabel;
		switch (HandSelected)
		{	
			case 0:
				CurrentHandItem = RightHandItem;
				CurrentHandNode = RightHand;
				CurrentPanelLabel = RightHandPanelLabel;
				break;
			case 1:
				CurrentHandItem = LeftHandItem;
				CurrentHandNode = LeftHand;
				CurrentPanelLabel = LeftHandPanelLabel;
				break;
			default:
				CurrentHandItem = RightHandItem;
				CurrentHandNode = RightHand;
				CurrentPanelLabel = RightHandPanelLabel;
				break;
		}

		if (CurrentHandItem != null)
		{
			CurrentHandNode.RemoveChild(CurrentHandItem);
			GetParent().AddChild(CurrentHandItem);
			CurrentHandItem.Position = Position;

			CurrentPanelLabel.Text = "Hand empty";

			CurrentHandItem = null;
		}

		switch (HandSelected)
		{	
			case 0:
				RightHandItem = CurrentHandItem;
				break;
			case 1:
				LeftHandItem = CurrentHandItem;
				break;
			default:
				RightHandItem = CurrentHandItem;
				break;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ChangeHand()
	{
		if (HandSelected != HandCount)
		{
			HandSelected = HandSelected++;
		}
		else
		{
			HandSelected = 0;
		}
	}
}
