//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class Ghost : CharacterBody3D
{	
	//Export variables
	[Export] public Camera3D Camera;
	[Export] public RayCast3D ExamineRay;
	[Export] public Label ExamineLabel;

	//Networking & multiplayer
	private bool Authority;

	//Characteristics
	private float MouseSensivity = 1f;

	private bool ControlsDisabled = false;

	private Vector3 InitialExamineVector;
	private Vector3 InitialExaminePosition;

	//Movement
	private float Speed = 5.0f;
	private Vector2 WalkDirection = Vector2.Zero;

	public override void _Ready()
	{
		Authority = GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
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
		if (Authority)
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
			RpcId(1, "SyncRotation", Rotation);
			if (ExamineLabel.Text != "")
			{
				if (InitialExamineVector - Camera.Rotation > new Vector3(10,10,10))
				{
					ExamineLabel.Text = "";
				}
			}
		}

		//Movement
		if (Event.IsAction("Forward") || Event.IsAction("Backward") || Event.IsAction("Right") || Event.IsAction("Left"))
		{
			WalkDirection = Input.GetVector("Left", "Right", "Forward", "Backward");
			RpcId(1, "SyncMoveDirection", WalkDirection);
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
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Authority || Multiplayer.IsServer())
		{
			Vector3 velocity = Velocity;
			Vector3 direction = (Transform.Basis * new Vector3(WalkDirection.X, 0, WalkDirection.Y)).Normalized();
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

	public void Sync()
	{
		if (Multiplayer.IsServer())
		{
			Rpc("SyncMoveDirection", WalkDirection);
			Rpc("SyncPosition", Position);
			Rpc("SyncVelocity", Velocity);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	private void SyncPosition(Vector3 SyncedPosition)
	{
		Position = SyncedPosition;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	private void SyncRotation(Vector3 SyncedRotation)
	{
		Rotation = SyncedRotation;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	private void SyncVelocity(Vector3 SyncedVelocity)
	{
		Velocity = SyncedVelocity;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	private void SyncMoveDirection(Vector2 SyncedDirection)
	{
		//This if needed to dont let cheaters make cheat which will give superspeed to them.
		if (SyncedDirection.X <= 1 && SyncedDirection.X >= -1 && SyncedDirection.Y <= 1 && SyncedDirection.Y >= -1)
		{
			WalkDirection = SyncedDirection;
		}
		else
		{
			GD.Print("CHEATER WARNING!");
		}
	}
}
