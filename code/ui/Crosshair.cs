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
			Message = Add.Label( "" );
		}
	}
}
