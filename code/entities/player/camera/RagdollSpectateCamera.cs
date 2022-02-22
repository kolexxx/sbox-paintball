using Sandbox;

namespace Paintball;

public class RagdollSpectateCamera : CameraMode, ISpectateCamera
{
	public override void Activated()
	{
		Position = CurrentView.Position;

		base.Activated();
	}

	public override void Update()
	{
	}
}
