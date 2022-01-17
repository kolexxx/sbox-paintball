using Sandbox;

namespace PaintBall;

public class FixedSpectateCamera : Camera, ISpectateCamera
{
	public override void Update()
	{
		throw new System.NotImplementedException();
	}

	public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer ) { }
}
