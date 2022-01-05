using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class TeamIndicator : Panel
	{
		public Label TeamName;

		public TeamIndicator()
		{
			TeamName = Add.Label( "None" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is not Player player )
				return;

			SetClass( "hidden", Local.Hud.GetChild( 10 ).IsVisible || (player.IsSpectator && !player.IsSpectatingPlayer) );

			TeamName.Text = player.CurrentPlayer == player ? player.Team.ToString() : player.CurrentPlayer.Client.Name;
		}
	}
}
