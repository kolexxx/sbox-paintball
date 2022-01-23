using Sandbox;

namespace Paintball;

public class RagdollSpectateCamera : Camera, ISpectateCamera
{
	public override void Activated()
	{
		Position = CurrentView.Position;

		base.Activated();
	}

	public override void Update()
	{
	}

	public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer ) { }
}
