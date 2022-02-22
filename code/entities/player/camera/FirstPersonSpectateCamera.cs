using Sandbox;

namespace Paintball;

public class FirstPersonSpectateCamera : CameraMode, ISpectateCamera
{
	private const float SMOOTH_SPEED = 25f;

	public override void Deactivated()
	{
		if ( Local.Pawn is not Player player )
			return;

		if ( player.CurrentPlayer.ActiveChild is Carriable weapon && weapon.ViewModelEntity != null )
			weapon.ViewModelEntity.EnableDrawing = false;

		if ( player.CameraMode is ThirdPersonSpectateCamera )
			return;

		if ( Host.IsClient && player.CurrentPlayer.IsValid() )
		{
			Local.Hud.RemoveClass( player.CurrentPlayer.Team.GetTag() );
			Local.Hud.AddClass( player.Team.GetTag() );
		}

		player.CurrentPlayer = null;
		player.StopLooking();
	}

	public void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer )
	{
		if ( oldPlayer.ActiveChild is Carriable oldWeapon && oldWeapon.ViewModelEntity != null )
			oldWeapon.ViewModelEntity.EnableDrawing = false;

		Local.Hud.RemoveClass( oldPlayer.Team.GetTag() );
		Local.Hud.AddClass( newPlayer.Team.GetTag() );

		if ( newPlayer.ActiveChild is Carriable newWeapon && newWeapon.ViewModelEntity != null )
			newWeapon.ViewModelEntity.EnableDrawing = true;

		Viewer = newPlayer;
	}

	public override void Update()
	{
		if ( Local.Pawn is not Player player )
			return;

		bool wantToUpdate = Input.Pressed( InputButton.Attack1 ) || Input.Pressed( InputButton.Attack2 );

		if ( !player.IsSpectatingPlayer || wantToUpdate )
			player.UpdateSpectatingPlayer( Input.Pressed( InputButton.Attack2 ) ? -1 : 1 );

		if ( player.CurrentPlayer.IsValid() )
		{
			Position = Vector3.Lerp( Position, player.CurrentPlayer.EyePosition, SMOOTH_SPEED * Time.Delta );
			Rotation = Rotation.Slerp( Rotation, player.CurrentPlayer.EyeRotation, SMOOTH_SPEED * Time.Delta );
		}
	}
}
