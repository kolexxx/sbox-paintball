using Sandbox;

namespace PaintBall
{
	public class ThirdPersonSpectateCamera : Camera, ISpectateCamera
	{
		private Vector3 DefaultPosition { get; set; }

		private const float LERP_MODE = 0;
		private const int CAMERA_DISTANCE = 120;

		private Rotation _targetRot;
		private Vector3 _targetPos;
		private Angles _lookAngles;

		public override void Activated()
		{
			base.Activated();

			Rotation = CurrentView.Rotation;
		}

		public override void Update()
		{
			if ( Local.Pawn is not Player player )
				return;

			bool wantToUpdate = Input.Pressed( InputButton.Attack1 ) || Input.Pressed( InputButton.Attack2 );

			if ( !player.IsSpectatingPlayer || wantToUpdate )
				player.UpdateSpectatingPlayer( Input.Pressed( InputButton.Attack2 ) ? -1 : 1 );

			_targetRot = Rotation.From( _lookAngles );
			Rotation = Rotation.Slerp( Rotation, _targetRot, 10 * RealTime.Delta * (1 - LERP_MODE) );

			_targetPos = GetSpectatePoint() + Rotation.Forward * -CAMERA_DISTANCE;
			Position = _targetPos;
		}

		private Vector3 GetSpectatePoint()
		{
			if ( Local.Pawn is not Player player || !player.IsSpectatingPlayer )
				return DefaultPosition;

			return player.CurrentPlayer.EyePos;
		}

		public override void BuildInput( InputBuilder input )
		{
			_lookAngles += input.AnalogLook;
			_lookAngles.roll = 0;

			base.BuildInput( input );
		}

		public override void Deactivated()
		{
			if ( Local.Pawn is not Player player )
				return;

			if ( Host.IsClient && player.CurrentPlayer.IsValid() )
			{
				Local.Hud.RemoveClass( player.CurrentPlayer.Team.GetString() );
				Local.Hud.AddClass( player.Team.GetString() );
			}

			player.CurrentPlayer = null;
		}

		public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer )
		{
			Local.Hud.RemoveClass( oldPlayer.Team.GetString() );
			Local.Hud.AddClass( newPlayer.Team.GetString() );
		}
	}
}
