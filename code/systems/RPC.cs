using Sandbox;

namespace PaintBall
{
	public partial class RPC
	{
		[ClientRpc]
		public static void ClientJoined( Client client )
		{
			Event.Run( PBEvent.Client.Joined, client );
		}

		[ClientRpc]
		public static void ClientDisconnected( Client client, NetworkDisconnectionReason reason )
		{
			Event.Run( PBEvent.Client.Disconnected, client, reason );
		}
	}
}
