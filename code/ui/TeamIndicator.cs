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

			SetClass( "hidden", player.LifeState == LifeState.Dead && (player.IsSpectator && !player.IsSpectatingPlayer) );

			TeamName.Text = player.CurrentPlayer.Team.ToString();
		}
	}
}
