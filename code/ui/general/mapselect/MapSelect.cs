using Sandbox;
using Sandbox.UI;

namespace Paintball.UI;

public class MapSelect : Panel
{
	public static MapSelect Instance;

	public MapSelect()
	{
		Instance = this;

		StyleSheet.Load( "/ui/general/mapselect/MapSelect.scss" );
	}

	[PBEvent.Game.StateChanged]
	public static void OnStateChanged( BaseState _, BaseState newState )
	{
		if ( !Host.IsClient || newState is not GameFinishedState )
			return;

		Local.Hud.AddChild<MapSelect>();
	}
}
