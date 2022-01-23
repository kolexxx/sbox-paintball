using Sandbox;

namespace Paintball;

public class FixedSpectateCamera : Camera, ISpectateCamera
{
	private int _index = 0;

	public override void Update()
	{
		throw new System.NotImplementedException();
	}

	public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer ) { }
}
