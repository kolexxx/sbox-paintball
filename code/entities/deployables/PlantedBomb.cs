using Sandbox;
using Sandbox.UI;

namespace PaintBall;

[Hammer.Skip]
public partial class PlantedBomb : ModelEntity, IUse, ILook
{
	[Net, Predicted] public Player Defuser { get; set; }
	[Net] public TimeSince TimeSinceStartedBeingDefused { get; set; } = 0f;
	public Player Planter { get; set; }
	public bool Disabled { get; set; }
	public Panel LookPanel { get; set; }
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
		else if ( Defuser?.Using != this )
		{
			Defuser = null;
			TimeSinceStartedBeingDefused = 0f;
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

	bool ILook.IsLookable( Entity viewer )
	{
		if ( viewer is not Player player )
			return false;

		return !_gameplayState.Disabled && player.Team == Team.Blue && (Defuser == null || Defuser == Local.Pawn);
	}

	void ILook.StartLook()
	{
		if ( Defuser == null )
		{
			LookPanel = Local.Hud.AddChild<WeaponLookAt>();
			(LookPanel as WeaponLookAt).Text.Text = "Press E to defuse";
			(LookPanel as WeaponLookAt).Icon.SetTexture( "ui/weapons/bomb.png" );
		}
		else if ( Defuser == Local.Pawn )
		{
			LookPanel = Local.Hud.AddChild<BombDefuse>();
		}
	}

	void ILook.Update()
	{
		if ( Defuser == Local.Pawn && LookPanel is WeaponLookAt )
		{
			LookPanel.Delete();
			LookPanel = Local.Hud.AddChild<BombDefuse>();
		}
		else if ( Defuser == null && LookPanel is not WeaponLookAt )
		{
			LookPanel?.Delete();
			LookPanel = Local.Hud.AddChild<WeaponLookAt>();
			(LookPanel as WeaponLookAt).Text.Text = "Press E to defuse";
			(LookPanel as WeaponLookAt).Icon.SetTexture( "ui/weapons/bomb.png" );
		}
	}

	void ILook.EndLook()
	{
		LookPanel.Delete();
	}
}
