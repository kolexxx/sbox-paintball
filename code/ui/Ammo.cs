using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class Ammo : Panel
{
	public Label Reserve;
	public Image AmmoIcon;
	public ProgressBar Clip;

	public Ammo()
	{
		AmmoIcon = Add.Image( "ui/ammo.png", "icon" );
		Reserve = Add.Label( string.Empty );

		AddChild( new ProgressBar( () =>
		 {
			 var player = Local.Pawn as Player;
			 if ( player == null )
				 return 0;

			 var weapon = player.CurrentPlayer.ActiveChild as Weapon;
			 if ( weapon == null )
				 return 0;

			 return weapon.AmmoClip / (float)weapon.ClipSize;
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

		var weapon = player.CurrentPlayer.ActiveChild as Weapon;

		if ( weapon == null )
		{
			Reserve.Text = "";

			return;
		}

		Reserve.Text = weapon.UnlimitedAmmo ? "∞" : $"{weapon.ReserveAmmo}";
	}
}
