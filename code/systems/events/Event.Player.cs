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

			public const string TeamChanged = "pb.player.teamchanged";

			/// <summary>
			/// Runs when a player changes teams.
			/// <para>Event is passed the <strong><see cref="PaintBall.Player"/></strong> instance of the player 
			/// who changed teams and the old <strong><see cref="PaintBall.Team"/></strong>.</para>
			/// </summary>
			public class TeamChangedAttribute : EventAttribute
			{
				public TeamChangedAttribute() : base( TeamChanged ) { }
			}
		}
	}
}
