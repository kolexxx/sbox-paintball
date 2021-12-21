using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace PaintBall
{
	public partial class Game : Sandbox.Game
	{
		public Hud Hud { get; set; }
		public static Game Instance
		{
			get => Current as Game;
		}

		[Net, Change( nameof( OnStateChanged ) )]
		public BaseState CurrentGameState { get; private set; }

		[ServerVar( "pb_min_players", Help = "The minimum players required to start." )]
		public static int MinPlayers { get; set; } = 2;

		private BaseState _lastGameState { get; set; }

		public Game()
		{
			if ( IsServer )
			{
				PrecacheAssets();

				Hud = new();
			}
		}

		[Event.Tick]
		private void Tick()
		{
			CurrentGameState?.Tick();
		}

		public void ChangeState( BaseState state )
		{
			Assert.NotNull( state );

			CurrentGameState?.Finish();
			CurrentGameState = state;
			CurrentGameState?.Start();
		}

		public override void ClientJoined( Client client )
		{
			var player = new Player();

			client.Pawn = player;

			Event.Run( PBEvent.Client.Joined, client );
			RPC.ClientJoined( client );

			CurrentGameState?.OnPlayerJoin( player );

			base.ClientJoined( client );
		}

		public override void ClientDisconnect( Client client, NetworkDisconnectionReason reason )
		{
			Event.Run( PBEvent.Client.Disconnected, client, reason );
			RPC.ClientDisconnected( client, reason );

			CurrentGameState?.OnPlayerLeave( client.Pawn as Player );

			base.ClientDisconnect( client, reason );
		}

		public override void MoveToSpawnpoint( Entity pawn )
		{
			if ( pawn is Player player )
			{
				Team team = player.Team;

				if ( player.Team == Team.None )
					team = (Team)Rand.Int( 1, 2 );

				var spawnpoints = Entity.All
									.OfType<PlayerSpawnPoint>()
									.Where( e => e.Team == team )
									.ToList();

				if ( spawnpoints.Count > 0 )
				{
					var spawnpoint = spawnpoints[Rand.Int( 0, spawnpoints.Count - 1 )];

					pawn.Transform = spawnpoint.Transform;

					return;
				}

				Log.Warning( $"Couldn't find team spawnpoint for {player}!" );
			}

			base.MoveToSpawnpoint( pawn );
		}

		public override void Shutdown()
		{
			CurrentGameState = null;
			_lastGameState = null;

			base.Shutdown();
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
			if ( CurrentGameState?.CanPlayerSuicide == false )
				return;

			base.DoPlayerSuicide( cl );
		}

		public override void OnKilled( Client client, Entity pawn )
		{
			Host.AssertServer();

			var attacker = pawn.LastAttacker?.Client?.Pawn as Player;
			var victim = pawn as Player;

			if ( attacker != null )
			{
				if ( attacker.Client != null )
				{
					Hud.AddKillFeed(
						attacker.Client.Name,
						client.Name,
						(pawn.LastAttackerWeapon as Weapon).Icon,
						attacker.Team, victim.Team,
						attacker.Client.PlayerId,
						client.PlayerId );
				}
				else
				{
					Hud.AddKillFeed(
						attacker.Name,
						client.Name,
						"killed",
						attacker.Team,
						victim.Team,
						attacker.NetworkIdent,
						client.PlayerId );
				}
			}
			else
			{
				Hud.AddKillFeed( "", client.Name, "", Team.None, victim.Team, 0, client.PlayerId );
			}
		}

		public void CleanUp()
		{
			Sandbox.Internal.Decals.RemoveFromWorld();

			foreach ( var projectile in All.OfType<Projectile>() )
			{
				if ( projectile.IsValid() )
					projectile.Delete();
			}

			foreach ( var weapon in All.OfType<Weapon>() )
			{
				if ( weapon.IsValid() && weapon.Owner == null )
					weapon.Delete();
			}

			ClientCleanUp();
		}

		[ClientRpc]
		public void ClientCleanUp()
		{
			foreach ( var ent in All.OfType<ModelEntity>() )
			{
				if ( ent.IsValid() && ent.IsClientOnly && ent is not BaseViewModel )
					ent.Delete();
			}
		}

		[Event.Entity.PostSpawn]
		private void EntityPostSpawn()
		{
			if ( IsServer )
				ChangeState( new WaitingForPlayersState() );
		}

		private void OnStateChanged()
		{
			if ( _lastGameState != CurrentGameState )
			{
				_lastGameState?.Finish();
				_lastGameState = CurrentGameState;
				_lastGameState.Start();
			}
		}

		private void PrecacheAssets()
		{
			var assets = FileSystem.Mounted.ReadJsonOrDefault<List<string>>( "paintball.assets.json" );

			foreach ( var asset in assets )
			{
				Log.Info( $"Precaching: {asset}" );
				Precache.Add( asset );
			}
		}
	}
}
