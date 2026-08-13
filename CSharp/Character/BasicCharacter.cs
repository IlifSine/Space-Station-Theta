//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class BasicCharacter : CharacterBody3D
{
	//Export variables
	[Export] private Camera3D Camera;
	private string GhostManagerPath = "/root/GameWorld/GhostManager";
	[Export] private bool IsGhostRole = false;
	[Export] public ExaminePanel examinePanel;
	[Export] public RayCast3D ExamineRay;
	[Export] public CanvasLayer canvasLayer;
	[Export] public VBoxContainer InternalPopupContainer;
	[Export] public Node3D ExternalPopupContainer;

	//PackedScene variables
	[Export] private PackedScene InternalPopupScene;
	[Export] private PackedScene ExternalPopupScene;

	private float MouseSensivity = 1.4f;
	private bool ControlsDisabled = false;

	//Movement
	[Export] private float Speed = 5.0f;
	private float Acceleration = 1.5f;
	private float SlowdownMultiplier = 0.5f;
	private Vector3 velocity;

	//Popup
	private float InternalPopupWaitTimeMultiplier = 0.2f;
	private float ExternalPopupWaitTimeMultiplier = 0.2f;
	private float ExternalPopupDistance = 0.2f;

	//Camera
	private float Yaw = 0;
	private float Pitch = 0;

	//Examine
	private Vector3 InitialExamineRotation;
	private Vector3 InitialExaminePosition;
	//private const float ExamineRotationHideThreshold = 0.5f;
	private const float ExaminePositionHideThreshold = 1f;
	
	public override void _Ready()
	{
		RefreshAuthority();
		if (IsGhostRole)
		{
			AddGhostRole();
		}
	}

	public override void _Input(InputEvent Event)
	{
		if (IsMultiplayerAuthority())
		{
			//Camera rotation
			if (Event is InputEventMouseMotion MouseEvent && Input.MouseMode == Input.MouseModeEnum.Captured)
			{
	   			Yaw += MouseEvent.Relative.X * MouseSensivity * -0.002f;
				Pitch += MouseEvent.Relative.Y * MouseSensivity * -0.002f;
		
				Pitch = Mathf.Clamp(Pitch, Mathf.DegToRad(-90), Mathf.DegToRad(90));
		
				Rotation = new Vector3(Pitch, Yaw, 0);

				/*//Examine hide
				if (ExamineLabel.Text != "")
				{
					double angle1 = InitialExamineRotation.euler_angles.x;
					double angle2 = vector2.euler_angles.x;

					double difference = (angle2 - angle1) % 360;

					//DEBUG
					GD.Print(diffEuler);
					if (diffEuler.X > ExamineQuaternionRotationHideThreshold || diffEuler.X < -ExamineQuaternionRotationHideThreshold || diffEuler.Z > ExamineQuaternionRotationHideThreshold || diffEuler.Z < -ExamineQuaternionRotationHideThreshold)
					{
						ExamineLabel.Text = "";
					}
				}*/
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
				if (ExamineRay.IsColliding() && ExamineRay.GetCollider() is ExamineStaticBody ExamineCollider)
				{
					examinePanel.TitleLabel.Text = ExamineCollider.ExamineName;
					examinePanel.DescLabel.Text = ExamineCollider.ExamineDesc;
					//InitialExamineRotation = Camera.Rotation;
					InitialExaminePosition = Position;
					examinePanel.Visible = true;
				}
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsMultiplayerAuthority())
		{
			velocity = Velocity;
			Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Backward");
			Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			if (direction != Vector3.Zero)
			{
				velocity.X = Mathf.MoveToward(Velocity.X, direction.X * Speed, Acceleration);
				velocity.Z = Mathf.MoveToward(Velocity.Z, direction.Z * Speed, Acceleration);
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, SlowdownMultiplier);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, SlowdownMultiplier);
			}

			//Examine hide
			if (examinePanel.Visible != false && InitialExaminePosition.DistanceTo(Position) > ExaminePositionHideThreshold)
			{
				examinePanel.Visible = false;
			}
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ChangeOwner(int PlayerId)
	{
		SetMultiplayerAuthority(PlayerId);
		RefreshAuthority();
	}

	private void RefreshAuthority()
	{
		if (IsMultiplayerAuthority())
		{
			Camera.MakeCurrent();
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else
		{
			Camera.ClearCurrent();
		}
	}

	private void AddGhostRole()
	{
		var ghostManager = GetNode<GhostManager>(GhostManagerPath);
		if (Multiplayer.IsServer())
		{
			ghostManager.AddGhostRole("Test Role", "Coder is testing", GetPath());
		}
	}
}
