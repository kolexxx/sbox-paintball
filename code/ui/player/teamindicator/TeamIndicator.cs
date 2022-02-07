using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class TeamIndicator : Panel
{
	public Label TeamName;

	public TeamIndicator()
	{
		StyleSheet.Load( "/ui/player/teamindicator/TeamIndicator.scss" );

		TeamName = Add.Label( "None" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player player )
			return;

		SetClass( "hidden", TeamSelect.Instance.IsVisible || (player.IsSpectator && !player.IsSpectatingPlayer) );

		TeamName.Text = player.CurrentPlayer.Team.GetName();
	}
}
