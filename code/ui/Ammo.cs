using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class Ammo : Panel
	{
		public Label AmmoCount;

		public Ammo()
		{
			AmmoCount = Add.Label( "100" );
		}

		public override void Tick()
		{
			var player = Local.Pawn as Player;

			if ( player == null )
				return;

			SetClass( "hidden", player.LifeState == LifeState.Dead && (player.IsSpectator && !player.IsSpectatingPlayer) );

			var weapon = player.CurrentPlayer.ActiveChild as Weapon;

			if ( weapon == null )
			{
				AmmoCount.Text = "";

				return;
			}

			AmmoCount.Text = $"{weapon.AmmoClip}/{weapon.ReserveAmmo}";
		}

	}

}
