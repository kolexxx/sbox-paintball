using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class WeaponLookAt : Panel
{
	public InputHint InputHint { get; set; }
	public Image Icon { get; set; }

	public WeaponLookAt()
	{
		StyleSheet.Load( "/ui/lookpanels/WeaponLookAt.scss" );

		InputHint = AddChild<InputHint>();
		InputHint.SetButton( InputButton.Use );
		InputHint.Context.Text = "Pick up";

		Icon = Add.Image( string.Empty, "icon" );
	}
}
