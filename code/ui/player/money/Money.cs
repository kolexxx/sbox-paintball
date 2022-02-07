using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class Money : Panel
{
	public Label Count;

	public Money()
	{
		StyleSheet.Load( "/ui/player/money/Money.scss" );

		Count = Add.Label( "0" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player player )
			return;

		Count.Text = $"${string.Format( "{0:n0}", player.CurrentPlayer.Money )}";
	}
}

