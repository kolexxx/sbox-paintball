using Sandbox;

namespace Paintball;

public class FreeSpectateCamera : CameraMode, ISpectateCamera
{
	private Angles _moveAngles;
	private Vector3 _moveInput;
	private float _moveSpeed;

	public override void Activated()
	{
		base.Activated();

		Position = CurrentView.Position;
		Rotation = CurrentView.Rotation;

		_moveAngles = Rotation.Angles();
	}

	public override void BuildInput( InputBuilder input )
	{
		_moveInput = input.AnalogMove;

		_moveSpeed = 1f;

		if ( input.Down( InputButton.Run ) )
			_moveSpeed = 5f;

		if ( input.Down( InputButton.Duck ) )
			_moveSpeed = 0.5f;

		_moveAngles += input.AnalogLook;
		_moveAngles.roll = 0;

		base.BuildInput( input );
	}

	public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer ) { }

	public override void Update()
	{
		if ( Local.Client == null )
			return;

		var Move = _moveInput.Normal * 300 * RealTime.Delta * Rotation * _moveSpeed;

		Position += Move;
		Rotation = Rotation.From( _moveAngles );
	}
}
