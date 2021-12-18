using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class Crosshair : Panel
	{
		public Label Message;

		public Crosshair()
		{
			StyleSheet.Load( "/ui/Crosshair.scss" );
			Message = Add.Label();
		}

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is not Player player )
				return;	

			SetClass( "hidden", Local.Hud.GetChild( 6 ).IsVisible || Local.Hud.GetChild( 9 ).IsVisible || (player.LifeState != LifeState.Alive && player.Camera is not FirstPersonSpectateCamera) );
		}
	}
}
