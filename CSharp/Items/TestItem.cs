//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;
using System;

public partial class TestItem : RigidBody3D, Interaction
{
	public void Interact()
	{
		
	}
	public void PickUp(CharacterBody3D Picker)
	{
		//This method calls a method with RPC (yes, this is kinda crutch but idk how do i should do normally)
		Rpc("RpcPickup");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RpcPickup()
	{
		QueueFree();
	}
}
