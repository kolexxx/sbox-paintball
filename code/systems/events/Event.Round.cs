using Sandbox;

namespace PaintBall
{
	public static partial class PBEvent
	{
		public static class Round
		{
			public const string Start = "pb.round.start";

			public class StartAttribute : EventAttribute
			{
				public StartAttribute() : base( Start ) { }
			}

			public const string End = "pb.round.end";

			public class EndAttribute : EventAttribute
			{
				public EndAttribute() : base( End ) { }
			}

			public const string New = "pb.round.new";

			public class NewAttribute : EventAttribute
			{
				public NewAttribute() : base( New ) { }
			}
		}
	}
}
