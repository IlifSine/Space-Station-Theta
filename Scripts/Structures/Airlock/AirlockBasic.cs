//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class AirlockBasic : StaticBody3D, Interaction
{
	[Export] private CollisionShape3D Collision;
	[Export] private AnimatedSprite3D Sprite;
	[Export] private Timer CloseWait;
	[Export] private Timer OpenTimer;

	private bool Open = false;
	private bool Busy = false;

	public void Interact()
	{
		ToggleAirlock();
	}

	public void PickUp(CharacterBody3D Picker)
	{
		ToggleAirlock();
	}

	private void ToggleAirlock()
	{
		if (!Busy)
		{
			if (Open)
			{
				Rpc("CloseAirlock");
			}
			else
			{
				Rpc("OpenAirlock");
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void CloseAirlock()
	{
		Busy = true;
		CloseWait.Stop();
		Sprite.PlayBackwards("Open");
		OpenTimer.Start();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void OpenAirlock()
	{
		Busy = true;
		Sprite.Play("Open");
		OpenTimer.Start();
	}

	public void OpenTimerTimeout()
	{
		if (Open)
		{
			Collision.SetDeferred("disabled", false);
			Open = false;
		}
		else
		{
			Collision.SetDeferred("disabled", true);
			Open = true;
			CloseWait.Start();
		}
		Busy = false;
	}

	private void CloseWaitTimeout()
	{
		if (!Busy)
		{
			Rpc("CloseAirlock");
		}
	}

	private void BodyEnteredHitbox(Node3D Body)
	{
		if (!Busy)
		{
			if (!Open)
			{
				//if a humanoid enters hitbox, door opens
				if (Body.IsInGroup("Humanoid"))
				{
					Rpc("OpenAirlock");
				}
			}
		}
	}
}
