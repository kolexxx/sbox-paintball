using Sandbox;

namespace PaintBall
{
	[Hammer.Skip]
	public partial class PlantedBomb : ModelEntity, IUse
	{
		[Net] public TimeUntil UntilDefuse { get; set; } = 5f;
		[Net] public TimeUntil UntilExplode { get; set; }
		[Net, Predicted] public bool Defused { get; set; } = false;
		[Net] public Player Planter { get; set; }
		[Net] public Player Defuser { get; set; }

		public override void Spawn()
		{
			base.Spawn();

			SetModel( $"models/red_ball/ball.vmdl" );
			PhysicsEnabled = false;
			UsePhysicsCollision = true;
			SetInteractsAs( CollisionLayer.All );
			SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );

			UntilExplode = GameplayState.BombDuration;
			(Game.Current.State as GameplayState).Bomb = this;

			(Game.Current.State as GameplayState).BombPlanted();
			Event.Run( PBEvent.Round.Bomb.Planted, this );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			(Game.Current.State as GameplayState).Bomb = this;
			Event.Run( PBEvent.Round.Bomb.Planted, this );

			Notification.Create( "Bomb has been planted!", 5 );
			Audio.Announce( "bomb_planted", Audio.Priority.Medium );
		}

		public void Tick()
		{
			if ( Defused )
			{
				Event.Unregister( this );
				return;
			}

			if ( UntilDefuse )
			{
				Defused = true;
				if ( IsServer )
					(Game.Current.State as GameplayState).BombDefused();
				
				Event.Run( PBEvent.Round.Bomb.Defused, this );
			}
			else if ( UntilExplode )
			{
				if ( IsServer )
					(Game.Current.State as GameplayState).BombExplode();

				Event.Run( PBEvent.Round.Bomb.Explode, this );
			}
			else if ( IsServer )
			{
				if ( Defuser == null )
					UntilDefuse = 5f;

				Defuser = null;
			}
		}

		public bool IsUsable( Entity user )
		{
			return user is Player player && player.Team == Team.Blue && Defuser == null;
		}

		public bool OnUse( Entity user )
		{
			Defuser = user as Player;
			return true;
		}
	}
}
