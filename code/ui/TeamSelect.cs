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

			Blue = Add.Button( "Join Blue", "blue", () => { Player.ChangeTeamCommand( Team.Blue ); } );
			Red = Add.Button( "Join Red", "red", () => { Player.ChangeTeamCommand( Team.Red ); } );
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
