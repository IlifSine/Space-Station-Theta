//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class ExternalPopup : Label3D
{
	[Export] private Timer DurationTimer;
	public void StartTimer(float Duration)
	{
		DurationTimer.WaitTime = Duration;
		DurationTimer.Start();
	}

	private void Timeout()
	{
		QueueFree();
	}
}
