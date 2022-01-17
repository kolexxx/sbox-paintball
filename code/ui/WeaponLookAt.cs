using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall;

public class WeaponLookAt : Panel
{
	public Label Text { get; set; }
	public Image Icon { get; set; }

	public WeaponLookAt()
	{
		StyleSheet.Load( "/ui/WeaponLookAt.scss" );
		Text = Add.Label( "Press E to pick up ", "text" );
		Icon = Add.Image( "", "icon" );
	}
}
