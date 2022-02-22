using Sandbox;

namespace Paintball;

public class FixedSpectateCamera : CameraMode, ISpectateCamera
{
	private PointCamera _currentPoint;
	private int _index = 0;

	public override void Activated()
	{
		base.Activated();

		if ( Map.SpectatePoints.Count == 0 )
			return;

		_currentPoint = Map.SpectatePoints[_index];
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

		Rotation = _currentPoint.Rotation;
		Position = _currentPoint.Position;
	}

	public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer ) { }
}
