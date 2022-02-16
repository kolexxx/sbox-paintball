using Sandbox;

namespace Paintball;

public class FixedSpectateCamera : Camera, ISpectateCamera
{
	private SpectatePoint _spectatePoint;
	private int _index = 0;

	public override void Activated()
	{
		base.Activated();

		if ( Map.SpectatePoints.Count == 0 )
			return;

		_spectatePoint = Map.SpectatePoints[_index];
	}

	public override void Update()
	{
		if ( Input.Pressed( InputButton.Attack1 ) )
			_index++;
		else if ( Input.Pressed( InputButton.Attack2 ) )
			_index--;

		if ( _index < 0 )
			_index = Map.SpectatePoints.Count - 1;
		else if ( _index >= Map.SpectatePoints.Count )
			_index = 0;

		Rotation = _spectatePoint.Rotation;
		Position = _spectatePoint.Position;
	}

	public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer ) { }
}
