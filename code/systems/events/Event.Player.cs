using Sandbox;

namespace PaintBall
{
	public static partial class PBEvent
	{
		public static class Player
		{
			public const string Killed = "pb.player.killed";

			/// <summary>
			/// Runs when a player dies.
			/// <para>Event is passed the <strong><see cref="PaintBall.Player"/></strong> instance of the player who died
			/// and the <strong><see cref="Sandbox.Entity"/></strong> instance of the attacker.</para>
			/// </summary>
			public class KilledAttribute : EventAttribute
			{
				public KilledAttribute() : base( Killed ) { }
			}

			public static class Spectating
			{
				public const string Changed = "pb.player.spectating.changed";

				/// <summary>
				/// Runs when the current spectated player changes.
				/// <para>Event is passed the <strong><see cref="PaintBall.Player"/></strong> instance of the old 
				/// spectated player and the <strong><see cref="PaintBall.Player"/></strong> instance of the new spactated player.</para>
				/// </summary>
				public class ChangedAttribute : EventAttribute
				{
					public ChangedAttribute() : base( Changed ) { }
				}
			}

			public static class Team
			{
				public const string Changed = "pb.player.team.changed";

				/// <summary>
				/// Runs when a player changes teams.
				/// <para>Event is passed the <strong><see cref="PaintBall.Player"/></strong> instance of the player 
				/// who changed teams and the old <strong><see cref="PaintBall.Team"/></strong>.</para>
				/// </summary>
				public class ChangedAttribute : EventAttribute
				{
					public ChangedAttribute() : base( Changed ) { }
				}
			}
		}
	}
}
