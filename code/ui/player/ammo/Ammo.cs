using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class Ammo : Panel
{
	public Image AmmoIcon;
	public ProgressBar Clip;
	public Label Reserve;

	public Ammo()
	{
		StyleSheet.Load( "/ui/player/ammo/Ammo.scss" );

		AmmoIcon = Add.Image( "ui/ammo.png", "icon" );
		Reserve = Add.Label( string.Empty );

		AddChild( new ProgressBar( () =>
		 {
			 var player = Local.Pawn as Player;
			 if ( player == null )
				 return 0;

			 var projectileWeapon = player.CurrentPlayer.ActiveChild as IProjectileWeapon;
			 if ( projectileWeapon == null )
				 return 0;

			 return projectileWeapon.AmmoClip / (float)projectileWeapon.Info.ClipSize;
		 } ) );

		Clip = GetChild( ChildrenCount - 1 ) as ProgressBar;
		Clip.AddClass( "clip" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as Player;

		if ( player == null )
			return;

		SetClass( "hidden", TeamSelect.Instance.IsVisible || (player.IsSpectator && !player.IsSpectatingPlayer) );

		if ( !IsVisible )
			return;

		var projectileWeapon = player.CurrentPlayer.ActiveChild as IProjectileWeapon;

		if ( projectileWeapon == null  )
		{
			Reserve.Text = "";

			return;
		}

		Reserve.Text = projectileWeapon.ReserveAmmo.ToString();
	}
}
