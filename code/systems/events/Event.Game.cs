using Sandbox;

namespace PaintBall;

public static partial class PBEvent
{
	public static class Game
	{
		public const string StateChanged = "pb.game.state.changed";

		/// <summary>
		/// Runs when the Game state changes.
		/// <para>Event is passed the <strong><see cref="PaintBall.BaseState"/></strong> instance of the old state
		/// and the <strong><see cref="PaintBall.BaseState"/></strong> instance of the new state.</para>
		/// </summary>
		public class StateChangedAttribute : EventAttribute
		{
			public StateChangedAttribute() : base( StateChanged ) { }
		}

		public const string MapInfoFetched = "pb.game.mapinfofetched";

		public class MapInfoFetchedAttribute : EventAttribute
		{
			public MapInfoFetchedAttribute() : base( MapInfoFetched ) { }
		}
	}
}
