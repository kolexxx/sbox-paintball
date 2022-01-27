using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Paintball.UI;

public class MapSelect : Panel
{
	public static MapSelect Instance;
	private ProgressBar _progressBar;

	public MapSelect()
	{
		Instance = this;

		StyleSheet.Load( "/ui/general/mapselect/MapSelect.scss" );

		AddChild( new ProgressBar( () =>
		{
			var state = Game.Current.State;

			return 1 - state.UntilStateEnds.Relative / state.StateDuration;
		} ) );

		_progressBar = GetChild( ChildrenCount - 1 ) as ProgressBar;
	}

	[PBEvent.Game.StateChanged]
	public static void OnStateChanged( BaseState _, BaseState newState )
	{
		if ( !Host.IsClient || newState is not MapSelectState )
			return;

		Local.Hud.AddChild<MapSelect>(); ;
	}

	public sealed class Entry : Panel
	{
		public Label Title;

		public Entry( string title, string thumbnail )
		{
			Title = Add.Label( title, "title" );

			Style.BackgroundImage = Texture.Load( thumbnail );

			AddEventListener( "onclick()", () =>
			{
				MapSelectState.SetVote( title );
			} );
		}
	}
}
