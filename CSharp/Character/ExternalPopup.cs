//Licensed under AGPL 3.0. Glory to communism!
using Godot;

public partial class ExternalPopup : Label3D
{
	[Export] Timer DurationTimer;
	public void StartTimer(float Duration)
	{
		DurationTimer.WaitTime = Duration;
		DurationTimer.Start();
	}

	void Timeout()
	{
		QueueFree();
	}
}
