using Sandbox;

namespace PaintBall
{
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
		}
	}
}
