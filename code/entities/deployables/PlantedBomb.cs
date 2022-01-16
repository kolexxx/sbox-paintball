using Sandbox;

namespace PaintBall;

[Hammer.Skip]
public partial class PlantedBomb : ModelEntity, IUse
{
	public Player Defuser { get; set; }
	public Player Planter { get; set; }
	public bool Disabled { get; set; }
	public TimeSince TimeSinceStartedBeingDefused { get; set; } = 0f;
	public TimeUntil TimeUntilExplode { get; set; }
	public RealTimeUntil UntilTickSound { get; set; }
	private GameplayState _gameplayState;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( $"models/red_ball/ball.vmdl" );
		PhysicsEnabled = false;
		UsePhysicsCollision = true;
		SetInteractsAs( CollisionLayer.All );
		SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );

		_gameplayState = Game.Current.State as GameplayState;
		_gameplayState.Bomb = this;
		_gameplayState.Planter = Planter;
		_gameplayState.Defuser = null;
		_gameplayState.Disabled = false;
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
			Disabled = true;
			_gameplayState.Disabled = true;
			_gameplayState.Defuser = Defuser;
			Event.Run( PBEvent.Round.Bomb.Defused, this );

			if ( _gameplayState.RoundState == RoundState.Bomb )
				_gameplayState.RoundStateFinish();
		}
		else if ( TimeUntilExplode )
		{
			Disabled = true;
			_gameplayState.Disabled = true;
			Defuser = null;
			_gameplayState.Defuser = null;
			Event.Run( PBEvent.Round.Bomb.Explode, this );

			if ( _gameplayState.RoundState == RoundState.Bomb )
				_gameplayState.RoundStateFinish();
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
		if ( !_gameplayState.Disabled && UntilTickSound )
		{
			Sound.FromEntity( "bomb_tick", this );
			UntilTickSound = 1f;
		}
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
