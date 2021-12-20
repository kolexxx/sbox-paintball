using Sandbox;
using Sandbox.UI;

namespace PaintBall
{
	[UseTemplate]
	public class TeamSelect : Panel
	{
		public Button Blue { get; set; }
		public Button Red { get; set; }
		private bool _open = true;

		public TeamSelect()
		{
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

		public void JoinBlue()
		{
			Player.ChangeTeamCommand( Team.Blue );
		}

		public void JoinRed()
		{
			Player.ChangeTeamCommand( Team.Red );
		}
	}
}
