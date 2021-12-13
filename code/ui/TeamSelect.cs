using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class TeamSelect : Panel
	{
		public Button Blue;
		public Button Red;
		private bool _open = true;

		public TeamSelect()
		{
			StyleSheet.Load( "/ui/TeamSelect.scss" );

			Blue = Add.Button( "Join Blue", "blue", () => { ConsoleSystem.Run( "changeteam 1" ); } );
			Red = Add.Button( "Join Red", "red", () => { ConsoleSystem.Run( "changeteam 2" ); } );
			Add.Label( "Press Q to open/close", "info" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Input.Pressed( InputButton.Menu ) )
				_open = !_open;

			SetClass( "open", _open );

			if ( !IsVisible )
				return;
		}
	}
}
