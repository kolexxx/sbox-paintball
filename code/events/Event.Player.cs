using Sandbox;

namespace PaintBall
{
	public static partial class PBEvent
	{
		public static class Player
		{
			public const string Killed = "pb.player.onkilled";

			public class KilledAttribute : EventAttribute
			{
				public KilledAttribute() : base( Killed ) { }
			}
		}
	}
}
