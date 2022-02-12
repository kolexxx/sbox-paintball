using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Paintball.UI;

public class ViewerCount : Panel
{
	public static ViewerCount Instance;
	private Image _icon;
	private Label _count;

	public ViewerCount()
	{
		Instance = this;

		StyleSheet.Load( "/ui/player/viewercount/ViewerCount.scss" );

		_icon = Add.Image( "ui/eye.png", "icon" );
		_count = Add.Label( "0", "count" );

		BindClass( "hidden", () => TeamSelect.Instance.IsVisible || (Local.Pawn as Player).CurrentPlayer.ViewerCount <= 0 );
	}

	public void Update( int newValue )
	{
		_count.Text = newValue.ToString();
	}
}

