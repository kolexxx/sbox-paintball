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
}
