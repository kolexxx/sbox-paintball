using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
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

		private BaseState LastGameState { get; set; }

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

			CurrentGameState?.OnPlayerJoin( player );

			base.ClientJoined( client );
		}

		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			CurrentGameState?.OnPlayerLeave( cl.Pawn as Player );

			base.ClientDisconnect( cl, reason );
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
			LastGameState = null;

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

		[ServerCmd( "changeteam", Help = "Changes the callers team" )]
		public static void ChangeTeamCommand()
		{
			Client client = ConsoleSystem.Caller;

			if ( client.PlayerId != 76561198087434609 )
				return;

			if ( client == null || client.Pawn is not PaintBall.Player player )
				return;

			player.SetTeam( player.Team == Team.Red ? Team.Blue : Team.Red );
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
					Hud.AddKillFeed( attacker.Client.Name, client.Name, (pawn.LastAttackerWeapon as Weapon).Icon, attacker.Team, victim.Team, attacker.Client.PlayerId, client.PlayerId );
				}
				else
				{
					Hud.AddKillFeed( attacker.Name, client.Name, "killed", attacker.Team, victim.Team, attacker.NetworkIdent, client.PlayerId );
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

			foreach(var weapon in All.OfType<Weapon>() )
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
				if ( ent.IsValid() && ent.IsClientOnly )
					ent.Delete();
			}
		}

		[Event.Entity.PostSpawn]
		private void EntityPostSpawn()
		{
			if ( IsServer )
			{
				ChangeState( new WaitingForPlayersState() );
			}
		}

		private void OnStateChanged()
		{
			if ( LastGameState != CurrentGameState )
			{
				LastGameState?.Finish();
				LastGameState = CurrentGameState;
				LastGameState.Start();
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
