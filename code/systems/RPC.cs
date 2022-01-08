using Sandbox;

namespace PaintBall
{
	public static partial class RPC
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

		[ClientRpc]
		public static void OnPlayerKilled( Player player )
		{
			if ( !player.IsValid() )
				return;

			Event.Run( PBEvent.Player.Killed, player );
		}
	}
}
