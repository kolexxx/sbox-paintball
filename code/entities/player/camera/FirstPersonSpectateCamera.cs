using Sandbox;

namespace PaintBall
{
	public class FirstPersonSpectateCamera : Camera, SpectateCamera
	{
		private const float SMOOTH_SPEED = 25f;

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

			base.Deactivated();
		}

		public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer )
		{
			Local.Hud.RemoveClass( oldPlayer.Team.GetString() );
			Local.Hud.AddClass( newPlayer.Team.GetString() );
		}

		public override void Update()
		{
			if ( Local.Pawn is not Player player )
				return;

			bool wantToUpdate = Input.Pressed( InputButton.Attack1 ) || Input.Pressed( InputButton.Attack2 );

			if ( !player.IsSpectatingPlayer || wantToUpdate )
			{
				player.UpdateSpectatingPlayer( Input.Pressed( InputButton.Attack2 ) ? -1 : 1 );

				Position = player.CurrentPlayer.EyePos;
				Rotation = player.CurrentPlayer.EyeRot;

				Viewer = player.CurrentPlayer;

				return;
			}

			if ( player.CurrentPlayer.IsValid() )
			{
				Position = Vector3.Lerp( Position, player.CurrentPlayer.EyePos, SMOOTH_SPEED * Time.Delta );
				Rotation = Rotation.Slerp( Rotation, player.CurrentPlayer.EyeRot, SMOOTH_SPEED * Time.Delta );
			}
		}
	}
}
