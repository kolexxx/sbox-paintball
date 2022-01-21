using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall;

public class BombDefuse : Panel
{
	public Label Instructions;
	public ProgressBar ProgressBar;

	public BombDefuse()
	{
		StyleSheet.Load( "/ui/BombDefuse.scss" );

		Instructions = Add.Label( "Keep holding E to defuse", "instructions" );
		AddChild( new ProgressBar( () =>
		  {
			  var bomb = (Game.Current.State as GameplayState).Bomb;
			  return (float)bomb.TimeSinceStartedBeingDefused.Relative / 5f;
		  } ) );

		ProgressBar = GetChild( ChildrenCount - 1 ) as ProgressBar;
	}
}
