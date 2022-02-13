using Paintball.UI;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Paintball;

[Hammer.Skip]
[Library( "paintball", Title = "PaintBall" )]
public partial class Game : Sandbox.Game
{
	public new static Game Current
	{
		get; private set;
	}

	[Net, Change] public BaseState State { get; private set; }
	public Map Map { get; set; }
	public Settings Settings { get; set; }
	private BaseState _lastState { get; set; }

	public Game()
	{
		Current = this;

		if ( IsServer )
			_ = new Hud();

		Map = new Map();
	}

	[Event.Tick]
	private void Tick()
	{
		State?.Tick();
	}

	public void ChangeState( BaseState state )
	{
		Assert.NotNull( state );

		var oldState = State;

		State?.Finish();
		State = state;
		State?.Start();

		Event.Run( PBEvent.Game.StateChanged, oldState, state );
	}

	public override bool CanHearPlayerVoice( Client source, Client dest )
	{
		return true;
	}

	public override void ClientJoined( Client client )
	{
		var player = new Player();

		client.Pawn = player;

		base.ClientJoined( client );

		State?.OnPlayerJoin( player );
		Event.Run( PBEvent.Client.Joined, client );
		RPC.ClientJoined( To.Everyone, client );
	}

	public override void ClientDisconnect( Client client, NetworkDisconnectionReason reason )
	{
		State?.OnPlayerLeave( client.Pawn as Player );
		Event.Run( PBEvent.Client.Disconnected, client.PlayerId, reason );
		RPC.ClientDisconnected( To.Everyone, client.PlayerId, reason );

		base.ClientDisconnect( client, reason );
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		if ( pawn is Player player )
		{
			Team team = player.Team;

			if ( player.Team == Team.None )
				team = (Team)Rand.Int( 1, 2 );

			var spawnpoints = Map.SpawnPoints
							 .Where( e => e.Team == team && !e.Occupied )
							 .ToList();

			if ( spawnpoints.Count > 0 )
			{
				var spawnpoint = spawnpoints[Rand.Int( 0, spawnpoints.Count - 1 )];

				if ( State is GameplayState )
					spawnpoint.Occupied = true;

				pawn.Transform = spawnpoint.Transform;

				return;
			}

			Log.Warning( $"Couldn't find team spawnpoint for {player}!" );
		}

		base.MoveToSpawnpoint( pawn );
	}

	public override void Shutdown()
	{
		base.Shutdown();

		State = null;
		_lastState = null;
	}

	public override void DoPlayerDevCam( Client player )
	{
		if ( player.PlayerId != 76561198087434609 )
			return;

		base.DoPlayerDevCam( player );
	}

	public override void DoPlayerNoclip( Client player )
	{
		if ( player.PlayerId != 76561198087434609 )
			return;

		base.DoPlayerNoclip( player );
	}

	public override void DoPlayerSuicide( Client cl )
	{
		if ( State?.CanPlayerSuicide == false )
			return;

		base.DoPlayerSuicide( cl );
	}

	[Event.Entity.PostSpawn]
	private void EntityPostSpawn()
	{
		if ( IsServer )
			ChangeState( new WaitingForPlayersState() );

		if ( Settings == null )
			Settings = new Settings();

		foreach ( var type in Library.GetAll<Weapon>() )
		{
			if ( !type.IsAbstract )
				_ = new ItemConfig( type );
		}
	}

	private void OnStateChanged()
	{
		if ( _lastState == State )
			return;

		var oldState = _lastState;

		_lastState?.Finish();
		_lastState = State;
		_lastState.Start();

		Event.Run( PBEvent.Game.StateChanged, oldState, _lastState );
	}
}
