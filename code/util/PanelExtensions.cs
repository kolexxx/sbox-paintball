using Sandbox.UI;

namespace Paintball;
public static class PanelExtensions
{
	public static Panel LastChild(this Panel panel )
	{
		return panel.GetChild( panel.ChildrenCount - 1 );
	}
}
