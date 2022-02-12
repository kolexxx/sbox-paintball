using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class Money : Panel
{
	public static Money Instance;
	public Label Count;
	public Label Difference;
	private RealTimeUntil _stopAnimation;
	private int _difference;

	// TODO: this is shit

	public Money()
	{
		Instance = this;

		StyleSheet.Load( "/ui/player/money/Money.scss" );

		Count = Add.Label( "0", "count" );
		Difference = Add.Label( string.Empty, "difference" );

		Difference.BindClass( "hide", () =>
		{
			if ( _stopAnimation )
			{
				_difference = 0;
				Count.Text = $"${string.Format( "{0:n0}", (Local.Pawn as Player).CurrentPlayer.Money )}";

				return true;
			}

			return false;
		} );

		Difference.BindClass( "show", () => !_stopAnimation );
		var player = Local.Pawn as Player;
	}

	public void AnimateChange( int difference )
	{
		if ( difference == 0 )
			return;

		_stopAnimation = 2f;
		_difference += difference;

		Difference.Text = $"${string.Format( "{0:n0}", _difference )}";
		Difference.SetClass( "minus", _difference < 0 );
		Difference.SetClass( "plus", _difference > 0 );
	}

	[PBEvent.Player.Spectating.Changed]
	private void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer )
	{
		_difference = 0;
		_stopAnimation = 0;

		Count.Text = $"${string.Format( "{0:n0}", (Local.Pawn as Player).CurrentPlayer.Money )}";
	}
}

