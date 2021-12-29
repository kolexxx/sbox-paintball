using Sandbox;

namespace PaintBall
{
	public static partial class PBEvent
	{
		public static class Client
		{
			public const string Joined = "pb.client.joined";

			public class JoinedAttribute : EventAttribute
			{
				public JoinedAttribute() : base( Joined ) { }
			}

			public const string Disconnected = "pb.client.disconnected";

			public class DisconnectedAttribute : EventAttribute
			{
				public DisconnectedAttribute() : base( Disconnected ) { }
			}
		}
	}
}
