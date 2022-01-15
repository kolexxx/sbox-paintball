using Sandbox;

namespace PaintBall
{
	[Hammer.Skip]
	public partial class PlantedBomb : ModelEntity, IUse
	{
		[Net] public Player Defuser { get; set; }
		[Net] public Player Planter { get; set; }
		[Net, Change( nameof( OnDisabled ) )] public bool Disabled { get; set; }
		public TimeSince TimeSinceStartedBeingDefused { get; set; } = 0f;
		public TimeUntil TimeUntilExplode { get; set; }
		public RealTimeUntil UntilTickSound { get; set; }
		private GameplayState _gameplayState;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( $"models/red_ball/ball.vmdl" );
			PhysicsEnabled = false;
			UsePhysicsCollision = false;
			SetInteractsAs( CollisionLayer.Solid );
			SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );

			_gameplayState = Game.Current.State as GameplayState;
			_gameplayState.Bomb = this;

			if ( _gameplayState.RoundState == RoundState.Play )
			{
				_gameplayState.RoundState = RoundState.Bomb;
				_gameplayState.RoundStateStart();
				TimeUntilExplode = _gameplayState.TimeLeft;
			}
			else
			{
				TimeUntilExplode = GameplayState.BombDuration;
			}
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			_gameplayState = Game.Current.State as GameplayState;
			_gameplayState.Bomb = this;

			Notification.Create( "Bomb has been planted!", 3 );
			Audio.Announce( "bomb_planted", Audio.Priority.Medium );
		}

		public void Tick()
		{
			if ( Disabled )
				return;

			if ( TimeSinceStartedBeingDefused >= 5f )
			{
				if ( _gameplayState.RoundState == RoundState.Bomb )
					_gameplayState.RoundStateFinish();

				Disabled = true;
				Event.Run( PBEvent.Round.Bomb.Defused, this );
			}
			else if ( TimeUntilExplode )
			{
				if ( _gameplayState.RoundState == RoundState.Bomb )
					_gameplayState.RoundStateFinish();

				Defuser = null;
				Disabled = true;
				Event.Run( PBEvent.Round.Bomb.Explode, this );
			}
			else
			{
				if ( Defuser == null )
					TimeSinceStartedBeingDefused = 0f;

				Defuser = null;
			}
		}

		[Event.Tick.Client]
		public void ClientTick()
		{
			if ( !Disabled && UntilTickSound )
			{
				Sound.FromEntity( "bomb_tick", this );
				UntilTickSound = 1f;
			}
		}

		private void OnDisabled()
		{
			if ( Defuser != null )
				Event.Run( PBEvent.Round.Bomb.Defused, this );
			else
				Event.Run( PBEvent.Round.Bomb.Explode, this );
		}

		bool IUse.IsUsable( Entity user )
		{
			return !Disabled && user is Player player && player.Team == Team.Blue && Defuser == null;
		}

		bool IUse.OnUse( Entity user )
		{
			Defuser = user as Player;
			return !Disabled;
		}
	}
}
