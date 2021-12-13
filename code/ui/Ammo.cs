using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class Ammo : Panel
	{
		public Label AmmoCount;
		public Image AmmoIcon;

		public Ammo()
		{
			AmmoCount = Add.Label( "100" );
			AmmoIcon = Add.Image( "", "icon" );
			AmmoIcon.SetTexture( "ui/ammo.png" );
		}

		public override void Tick()
		{
			var player = Local.Pawn as Player;

			if ( player == null )
				return;

			SetClass( "hidden", Local.Hud.GetChild( 9 ).IsVisible || (player.IsSpectator && !player.IsSpectatingPlayer) );

			var weapon = player.CurrentPlayer.ActiveChild as Weapon;

			if ( weapon == null )
			{
				AmmoCount.Text = "";

				return;
			}

			string reserve = weapon.UnlimitedAmmo ? "∞" : $"{weapon.ReserveAmmo}";
			AmmoCount.Text = $"{weapon.AmmoClip}/{reserve}";
		}

	}

}
