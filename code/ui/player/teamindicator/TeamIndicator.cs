using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class TeamIndicator : Panel
{
	private Label _teamName;

	public TeamIndicator()
	{
		StyleSheet.Load( "/ui/player/teamindicator/TeamIndicator.scss" );

		_teamName = Add.Label( "None" );

		BindClass( "hidden", () =>
		{
			var player = Local.Pawn as Player;

			if ( TeamSelect.Instance.IsVisible )
				return true;

			if ( player.IsSpectator && !player.IsSpectatingPlayer )
				return true;

			return false;
		} );
	}

	[PBEvent.Player.Spectating.Changed]
	private void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer )
	{
		var player = Local.Pawn as Player;

		_teamName.Text = player.CurrentPlayer.Team.GetName();
	}

	[PBEvent.Player.Team.Changed]
	private void OnPlayerTeamChanged( Player player, Team oldTeam )
	{
		if ( player != (Local.Pawn as Player).CurrentPlayer )
			return;

		_teamName.Text = player.Team.GetName();
	}
}
