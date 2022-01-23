using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall;

[UseTemplate]
public class SpectatedPlayerInfo : Panel
{
	private static SpectatedPlayerInfo s_current;
	private Panel _playerInfo;
	private Panel _spectatorControls;

	public SpectatedPlayerInfo( Player player )
	{
	}

	[PBEvent.Player.Spectating.Changed]
	private static void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer )
	{
		return;

		if ( oldPlayer != null )
			s_current.Delete();

		if ( newPlayer == null )
			return;

		Local.Hud.AddChild( new SpectatedPlayerInfo( newPlayer ) );
		s_current = Local.Hud.GetChild( Local.Hud.ChildrenCount - 1 ) as SpectatedPlayerInfo;
	}
}
