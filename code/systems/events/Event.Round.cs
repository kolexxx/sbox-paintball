using Sandbox;

namespace PaintBall;

public static partial class PBEvent
{
	public static class Round
	{
		public const string Start = "pb.round.start";

		/// <summary>
		/// Runs when the play duration of the current round has started.
		/// </summary>
		public class StartAttribute : EventAttribute
		{
			public StartAttribute() : base( Start ) { }
		}

		public const string End = "pb.round.end";

		/// <summary>
		/// Runs when the play duration of the current round has ended.
		/// <para>Event is passed the <strong><see cref="PaintBall.Team"/></strong> which won the round.</para>
		/// </summary>
		public class EndAttribute : EventAttribute
		{
			public EndAttribute() : base( End ) { }
		}

		public const string New = "pb.round.new";

		/// <summary>
		/// Runs at the beginning of a new round.
		/// </summary>
		public class NewAttribute : EventAttribute
		{
			public NewAttribute() : base( New ) { }
		}

		public static class Bomb
		{
			public const string Planted = "pb.round.bomb.planted";

			/// <summary>
			/// Runs when the bomb has been planted.
			/// <para>Event is passed the <strong><see cref="PaintBall.PlantedBomb"/></strong> instance 
			/// of the bomb.</para>
			/// </summary>
			public class PlantedAttribute : EventAttribute
			{
				public PlantedAttribute() : base( Planted ) { }
			}

			public const string Explode = "pb.round.bomb.explode";

			/// <summary>
			/// Runs when the bomb has exploded.
			/// <para>Event is passed the <strong><see cref="PaintBall.PlantedBomb"/></strong> instance 
			/// of the bomb. Gets called before <strong><see cref="PaintBall.PBEvent.Round.EndAttribute"/></strong>
			/// if the bomb explosion triggered the round to end.</para>
			/// </summary>
			public class ExplodeAttribute : EventAttribute
			{
				public ExplodeAttribute() : base( Explode ) { }
			}

			public const string Defused = "pb.round.bomb.defused";

			/// <summary>
			/// Runs when the bomb has been defused.
			/// <para>Event is passed the <strong><see cref="PaintBall.PlantedBomb"/></strong> instance 
			/// of the bomb. Gets called before <strong><see cref="PaintBall.PBEvent.Round.EndAttribute"/></strong>
			/// if the bomb defuse triggered the round to end.</para>
			/// </summary>
			public class DefusedAttribute : EventAttribute
			{
				public DefusedAttribute() : base( Defused ) { }
			}
		}
	}
}
