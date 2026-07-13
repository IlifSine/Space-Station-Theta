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
	private bool Authority;

	private float MouseSensivity = 1.4f;

	private bool ControlsDisabled = false;

	private Vector3 InitialExamineVector;
	private Vector3 InitialExaminePosition;

	//Movement
	private float Speed = 5.0f;
	private float Acceleration = 1.5f;
	private float SlowdownMultiplier = 0.5f;
	private Vector3 velocity;

	//Popup
	private float InternalPopupWaitTimeMultiplier = 0.2f;
	private float ExternalPopupWaitTimeMultiplier = 0.2f;
	private float ExternalPopupDistance = 0.2f;

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
			/*if (ExamineLabel.Text != "")
			{
				if (InitialExamineVector - Camera.Rotation > new Vector3(10,10,10))
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
		}
		Velocity = velocity;
		MoveAndSlide();

	}

	/// <summary>
	/// Shows a control label popup with some text on the local client. The popup will automatically disappear after a duration based on the length of the text.
	/// </summary>
	/// <param name="Text">Text to display in the popup</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	private void ShowInternalPopup(string Text)
	{
		var PopupInstance = InternalPopupScene.Instantiate<InternalPopup>();
		InternalPopupContainer.AddChild(PopupInstance);
		PopupInstance.Text = Text;
		PopupInstance.StartTimer(Text.Length * InternalPopupWaitTimeMultiplier);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
	private void ShowExternalPopup(string Text)
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

	private void LocalShowExternalPopup(string Text)
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
}
