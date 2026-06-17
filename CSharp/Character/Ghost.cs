//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class Ghost : CharacterBody3D
{	
	//Export variables
	[Export] public Camera3D Camera;
	[Export] public RayCast3D ExamineRay;
	[Export] public Label ExamineLabel;
	[Export] public CanvasLayer canvasLayer;
	[Export] public VBoxContainer InternalPopupContainer;
	[Export] public Node3D ExternalPopupContainer;

	//PackedScene variables
	[Export] private PackedScene InternalPopupScene;
	[Export] private PackedScene ExternalPopupScene;

	//Networking & multiplayer
	bool Authority;

	//Characteristics
	float MouseSensivity = 1.4f;

	bool ControlsDisabled = false;

	Vector3 InitialExamineVector;
	Vector3 InitialExaminePosition;

	//Movement
	float Speed = 5.0f;
	Vector2 WalkDirection = Vector2.Zero;

	//Popup
	float InternalPopupWaitTimeMultiplier = 0.2f;
	float ExternalPopupWaitTimeMultiplier = 0.2f;
	float ExternalPopupDistance = 0.2f;

	public override void _Ready()
	{
		if (IsMultiplayerAuthority())
		{
			Camera.MakeCurrent();
		}
		else
		{
			Camera.ClearCurrent();
			canvasLayer.QueueFree();
		}
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent Event)
	{
		if (IsMultiplayerAuthority())
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
		if (IsMultiplayerAuthority() || Multiplayer.IsServer())
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

	/// <summary>
	/// Shows a control label popup with some text on the local client. The popup will automatically disappear after a duration based on the length of the text.
	/// </summary>
	/// <param name="Text">Text to display in the popup</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	void ShowInternalPopup(string Text)
	{
		var PopupInstance = InternalPopupScene.Instantiate<InternalPopup>();
		InternalPopupContainer.AddChild(PopupInstance);
		PopupInstance.Text = Text;
		PopupInstance.StartTimer(Text.Length * InternalPopupWaitTimeMultiplier);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	void ShowExternalPopup(string Text)
	{
		if (IsMultiplayerAuthority())
		{
			Rpc("ShowExternalPopup", Text);
			LocalShowExternalPopup(Text);
		}
		else
		{
			LocalShowExternalPopup(Text);
		}
	}

	void LocalShowExternalPopup(string Text)
	{
		if (ExternalPopupContainer.GetChildCount() > 0)
		{
			var PopupInstance = ExternalPopupScene.Instantiate<ExternalPopup>();
			ExternalPopupContainer.AddChild(PopupInstance);
			PopupInstance.Text = Text;
			PopupInstance.StartTimer(Text.Length * ExternalPopupWaitTimeMultiplier);
			PopupInstance.Position = new Vector3(0, ExternalPopupDistance * ExternalPopupContainer.GetChildCount(), 0);
		}
		else
		{
			var PopupInstance = ExternalPopupScene.Instantiate<ExternalPopup>();
			ExternalPopupContainer.AddChild(PopupInstance);
			PopupInstance.Text = Text;
			PopupInstance.StartTimer(Text.Length * ExternalPopupWaitTimeMultiplier);
		}
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
	void SyncPosition(Vector3 SyncedPosition)
	{
		Position = SyncedPosition;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	void SyncRotation(Vector3 SyncedRotation)
	{
		Rotation = SyncedRotation;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	void SyncVelocity(Vector3 SyncedVelocity)
	{
		Velocity = SyncedVelocity;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	void SyncMoveDirection(Vector2 SyncedDirection)
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
