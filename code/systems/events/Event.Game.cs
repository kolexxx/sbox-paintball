using Sandbox;

namespace Paintball;

public static partial class PBEvent
{
	public static class Game
	{
		public const string StateChanged = "pb.game.state.changed";

		/// <summary>
		/// Runs when the Game state changes.
		/// <para>Event is passed the <strong><see cref="Paintball.BaseState"/></strong> instance of the old state
		/// and the <strong><see cref="Paintball.BaseState"/></strong> instance of the new state.</para>
		/// </summary>
		public class StateChangedAttribute : EventAttribute
		{
			public StateChangedAttribute() : base( StateChanged ) { }
		}

		public static class Map
		{
			public const string InfoFetched = "pb.game.map.infofetched";

			public class InfoFetchedAttribute : EventAttribute
			{
				public InfoFetchedAttribute() : base( InfoFetched ) { }
			}

			
		}

		public const string SettingsLoaded = "pb.game.settingsloaded";

		public class SettingsLoadedAttribute : EventAttribute
		{
			public SettingsLoadedAttribute() : base( SettingsLoaded ) { }
		}
	}
}
