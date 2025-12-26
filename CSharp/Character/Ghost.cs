//Licensed under AGPL 3.0
using System;
using Godot;

public partial class Ghost : CharacterBody3D
{	
	//Export variables
	[Export] public MultiplayerSynchronizer LocalSynchronizer;
	[Export] public Camera3D Camera;
	[Export] public RayCast3D ExamineRay;
	[Export] public Label ExamineLabel;

	//Networking & multiplayer
	private bool Authority;

	//Characteristics
	private float Speed = 5.0f;
	private float MouseSensivity = 1f;

	private bool ControlsDisabled = false;

	private Vector3 InitialExamineVector;
	private Vector3 InitialExaminePosition;

	public override void _Ready()
	{
		Authority = LocalSynchronizer.GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
		if (Authority)
		{
			Camera.MakeCurrent();
		}
		else
		{
			Camera.ClearCurrent();
		}
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent Event)
	{
		//Camera rotation
		if (Event is InputEventMouseMotion MouseEvent && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			RotateY(MouseEvent.Relative.X * MouseSensivity * -0.002f);
			Camera.RotateX(MouseEvent.Relative.Y * MouseSensivity * -0.002f);
			Camera.Rotation = new Vector3
			(
				Mathf.Clamp(Camera.Rotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90)),
				Camera.Rotation.Y,
				Camera.Rotation.Z
			);
			//if (ExamineLabel.Text != "")
			//{
			//	if (InitialExamineVector - Camera.Rotation > new Vector3(10,10,10))
			//	{
			//		ExamineLabel.Text = "";
			//	}
			//}
		}
		if (Event.IsActionPressed("ShowCursor"))
		{
			//Disable most controls and show mouse cursor when alt is hold
			ControlsDisabled = true;
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		if (Event.IsActionReleased("ShowCursor"))
		{
			//Enable most controls and hide mouse cursor when alt isn't hold
			ControlsDisabled = false;
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		if (Event.IsActionPressed("Examine"))
		{
			if (ExamineRay.IsColliding())
			{
				Node ExamineCollider = ExamineRay.GetCollider() as Node;
				ExamineLabel.Text = ExamineCollider.EditorDescription;
				InitialExamineVector = Camera.Rotation;
				InitialExaminePosition = Position;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Authority)
		{
			Vector3 velocity = Velocity;

			if (ExamineLabel.Text != "" && Math.Abs(InitialExaminePosition.X) > 2 || Math.Abs(InitialExaminePosition.Y) > 2 || Math.Abs(InitialExaminePosition.Z) > 2)
			{
				ExamineLabel.Text = "";
			}

			Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Backward");
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			}

			Velocity = velocity;

		}
		MoveAndSlide();

	}
}
