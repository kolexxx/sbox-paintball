using Sandbox;

namespace Paintball;

public static partial class RPC
{
	[ClientRpc]
	public static void ClientJoined( Client client )
	{
		Event.Run( PBEvent.Client.Joined, client );
	}

	[ClientRpc]
	public static void ClientDisconnected( long playerId, NetworkDisconnectionReason reason )
	{
		Event.Run( PBEvent.Client.Disconnected, playerId, reason );
	}

	[ClientRpc]
	public static void OnPlayerKilled( Player player )
	{
		if ( !player.IsValid() )
			return;

		Event.Run( PBEvent.Player.Killed, player );
	}

	[ClientRpc]
	public static void OnRoundStateChanged( RoundState roundState, Team winner = Team.None )
	{
		if ( Game.Current.State is not GameplayState gameplayState )
			return;

		gameplayState.RoundState = roundState;

		switch ( roundState )
		{
			case RoundState.Freeze:

				Event.Run( PBEvent.Round.New );

				break;

			case RoundState.Play:

				Event.Run( PBEvent.Round.Start );

				break;

			case RoundState.End:

				Event.Run( PBEvent.Round.End, winner );

				break;
		}

		gameplayState.RoundStateStart();
	}
}
