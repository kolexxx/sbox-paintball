using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class SpectatorControls : Panel
	{
		public SpectatorControls()
		{
			Add.Label( "Spacebar - switch camera" );
			Add.Label( "Attack (1 or 2) - switch player" );

			BindClass( "hidden", () => Local.Hud.GetChild( 10 ).IsVisible );
		}

		public override void Tick()
		{
			base.Tick();

			var player = Local.Pawn as Player;

			if ( player == null )
				return;

			SetClass( "hidden", !player.IsSpectator );
		}
	}
}
