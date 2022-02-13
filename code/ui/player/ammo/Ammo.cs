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

			 var carriable = player.CurrentPlayer.ActiveChild as Carriable;
			 if ( carriable == null || carriable.Info is not ProjectileWeaponInfo info )
				 return 0;

			 return carriable.AmmoClip / (float)info.ClipSize;
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

		var carriable = player.CurrentPlayer.ActiveChild as Carriable;

		if ( carriable == null || carriable.Info is not ProjectileWeaponInfo info )
		{
			Reserve.Text = "";

			return;
		}

		Reserve.Text = info.ReserveAmmo.ToString();
	}
}
