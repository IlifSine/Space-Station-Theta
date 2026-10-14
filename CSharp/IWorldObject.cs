using Godot;
using System;

public partial interface IWorldObject
{
	[Export] public string ExamineName { get; set; }
	[Export] public string ExamineDesc { get; set; }

	public void Interact();
}
