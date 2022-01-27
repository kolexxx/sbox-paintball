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
	private readonly List<Entry> _entries = new();
	private Panel _mapList;

	public MapSelect()
	{
		Instance = this;

		StyleSheet.Load( "/ui/general/mapselect/MapSelect.scss" );

		AddChild( new ProgressBar( () =>
		{
			var state = Game.Current.State;

			return state.UntilStateEnds.Relative / state.StateDuration;
		} ) );

		_progressBar = GetChild( 0 ) as ProgressBar;
		_mapList = Add.Panel( "map-list" );
	}

	public void LoadMaps()
	{
		var mapImages = (Game.Current.State as MapSelectState).MapImages;

		foreach ( KeyValuePair<string, string> kvp in mapImages )
		{
			if ( _entries.Exists( ( mapPanel ) => mapPanel.Title.Text == kvp.Key ) )
				continue;

			_mapList.AddChild( new Entry( kvp.Key, kvp.Value ) );
			_entries.Add( _mapList.GetChild( _mapList.ChildrenCount - 1 ) as Entry );
		}
	}

	public sealed class Entry : Panel
	{
		public Label Title;
		public Label VoteCount;

		public Entry( string title, string thumbnail )
		{
			Title = Add.Label( title, "title" );
			VoteCount = Add.Label( "0", "vote-count" );

			Style.BackgroundImage = Texture.Load( thumbnail );

			AddEventListener( "onclick", () =>
			{
				MapSelectState.SetVote( title );
			} );
		}

		public override void Tick()
		{
			base.Tick();

			// ugly ass code
			SetClass( "voted", (Game.Current.State as MapSelectState).PlayerIdVote[Local.Pawn.Client.PlayerId] == Title.Text );
			VoteCount.Text = (Game.Current.State as MapSelectState).VoteCount[Title.Text].ToString();
		}
	}
}
