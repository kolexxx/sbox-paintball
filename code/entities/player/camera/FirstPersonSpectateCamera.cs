using Sandbox;

namespace PaintBall;

public class FirstPersonSpectateCamera : Camera, ISpectateCamera
{
	private const float SMOOTH_SPEED = 25f;

	public override void Deactivated()
	{
		if ( Local.Pawn is not Player player )
			return;

		if ( player.CurrentPlayer.ActiveChild is Weapon weapon && weapon.ViewModelEntity != null )
			weapon.ViewModelEntity.EnableDrawing = false;

		if ( player.Camera is ThirdPersonSpectateCamera )
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
		if ( oldPlayer.ActiveChild is Weapon oldWeapon && oldWeapon.ViewModelEntity != null )
			oldWeapon.ViewModelEntity.EnableDrawing = false;

		Local.Hud.RemoveClass( oldPlayer.Team.GetString() );
		Local.Hud.AddClass( newPlayer.Team.GetString() );

		if ( newPlayer.ActiveChild is Weapon newWeapon && newWeapon.ViewModelEntity != null )
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
			Position = Vector3.Lerp( Position, player.CurrentPlayer.EyePos, SMOOTH_SPEED * Time.Delta );
			Rotation = Rotation.Slerp( Rotation, player.CurrentPlayer.EyeRot, SMOOTH_SPEED * Time.Delta );
		}
	}
}
