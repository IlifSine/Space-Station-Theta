//open-source EULA/CLA, see full text in LICENSE.txt
using Godot;
using System;

public interface Interaction
{
	void PickUp(CharacterBody3D Picker);

	void Interact();
}
