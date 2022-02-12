using Sandbox;

namespace Paintball;

public sealed class ViewModel : BaseViewModel
{
	private float _swingInfluence => 0.05f;
	private float _returnSpeed => 5.0f;
	private float _maxOffsetLength => 10.0f;
	private float _bobCycleTime => 5f;
	private Vector3 _bobDirection => new Vector3( 0.0f, 1.0f, 0.5f );
	private Vector3 _swingOffset;
	private float _lastPitch;
	private float _lastYaw;
	private float _bobAnim;

	private bool _activated = false;

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		if ( Local.Pawn is not Player player )
			return;

		player = player.CurrentPlayer;

		if ( !player.IsValid() )
			return;

		if ( !_activated )
		{
			_lastPitch = camSetup.Rotation.Pitch();
			_lastYaw = camSetup.Rotation.Yaw();

			_activated = true;
		}

		Position = camSetup.Position;
		Rotation = camSetup.Rotation;

		camSetup.ViewModel.FieldOfView = FieldOfView;

		var playerVelocity = player.Velocity;


		var controller = player.GetActiveController();
		if ( controller != null && controller.HasTag( "noclip" ) )
		{
			playerVelocity = Vector3.Zero;
		}


		var newPitch = Rotation.Pitch();
		var newYaw = Rotation.Yaw();

		var pitchDelta = Angles.NormalizeAngle( newPitch - _lastPitch );
		var yawDelta = Angles.NormalizeAngle( _lastYaw - newYaw );

		var verticalDelta = playerVelocity.z * Time.Delta;
		var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
		verticalDelta *= (1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).y ));
		pitchDelta -= verticalDelta * 1;

		var offset = CalcSwingOffset( pitchDelta, yawDelta );
		offset += CalcBobbingOffset( playerVelocity );

		Position += Rotation * offset;

		_lastPitch = newPitch;
		_lastYaw = newYaw;
	}

	private Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
	{
		Vector3 swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

		_swingOffset -= _swingOffset * _returnSpeed * Time.Delta;
		_swingOffset += (swingVelocity * _swingInfluence);

		if ( _swingOffset.Length > _maxOffsetLength )
		{
			_swingOffset = _swingOffset.Normal * _maxOffsetLength;
		}

		return _swingOffset;
	}

	private Vector3 CalcBobbingOffset( Vector3 velocity )
	{
		_bobAnim += Time.Delta * _bobCycleTime;

		var twoPI = System.MathF.PI * 2.0f;

		if ( _bobAnim > twoPI )
		{
			_bobAnim -= twoPI;
		}

		var speed = new Vector2( velocity.x, velocity.y ).Length;
		speed = speed > 10.0 ? speed : 0.0f;
		var offset = _bobDirection * (speed * 0.005f) * System.MathF.Cos( _bobAnim );
		offset = offset.WithZ( -System.MathF.Abs( offset.z ) );

		return offset;
	}
}
