//Licensed under AGPL 3.0. Glory to communism!
using Godot;
using System;

public partial class Item : RigidBody3D, IWorldObject
{
	[Export] public string ExamineName { get; set; }
	[Export] public string ExamineDesc { get; set; }

	public void Interact(){}
}
