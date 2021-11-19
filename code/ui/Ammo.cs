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

			var weapon = player.ActiveChild as Weapon;

			if ( weapon == null )
				return;

			AmmoCount.Text = $"{weapon.AmmoClip}";
		}
		
	}

}
