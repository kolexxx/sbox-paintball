using Sandbox;

namespace Paintball;

public static partial class PBEvent
{
	public static class Client
	{
		public const string Joined = "pb.client.joined";

		/// <summary>
		/// Runs when a client joins.
		/// <para>Event is passed the <strong><see cref="Sandbox.Client"/></strong> instance 
		/// of the player who connected.</para>
		/// </summary>
		public class JoinedAttribute : EventAttribute
		{
			public JoinedAttribute() : base( Joined ) { }
		}

		public const string Disconnected = "pb.client.disconnected";

		/// <summary>
		/// Runs when a client disconnects.
		/// <para>Event is passed the <strong><see cref="long"/></strong> <strong>PlayerId</strong> of the client who disconnected and
		/// the <strong><see cref="Sandbox.NetworkDisconnectionReason"/></strong>.</para>
		/// </summary>
		public class DisconnectedAttribute : EventAttribute
		{
			public DisconnectedAttribute() : base( Disconnected ) { }
		}
	}
}
