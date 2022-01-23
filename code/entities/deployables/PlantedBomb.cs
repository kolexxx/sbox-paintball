using Sandbox;
using Sandbox.UI;

namespace Paintball;

[Hammer.Skip]
public partial class PlantedBomb : ModelEntity, IUse, ILook
{
	[Net, Change] public Player Defuser { get; set; }
	[Net] public TimeSince TimeSinceStartedBeingDefused { get; set; } = 0f;
	public Player Planter { get; set; }
	public TimeUntil TimeUntilExplode { get; set; }
	public bool Disabled { get; set; }
	public Panel LookPanel { get; set; }
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
	}

	public void Initialize()
	{
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
			Event.Run( PBEvent.Round.Bomb.Planted, this );
			OnPlanted( Planter );

			TimeUntilExplode = GameplayState.BombDuration;
		}
	}

	public void Tick()
	{
		if ( Disabled )
			return;

		if ( Defuser?.Using != this )
		{
			Defuser = null;
			TimeSinceStartedBeingDefused = 0f;
		}

		if ( TimeSinceStartedBeingDefused >= 5f )
		{
			Disabled = true;

			Event.Run( PBEvent.Round.Bomb.Defused, this );
			OnDisabled( Defuser );
		}
		else if ( TimeUntilExplode )
		{
			Disabled = true;
			Defuser = null;

			Event.Run( PBEvent.Round.Bomb.Explode, this );
			OnDisabled( Defuser );
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

	[ClientRpc]
	public void OnPlanted( Player planter )
	{
		Planter = planter;
		_gameplayState = Game.Current.State as GameplayState;

		if ( _gameplayState.RoundState == RoundState.Play )
			_gameplayState.RoundState = RoundState.Bomb;

		_gameplayState.Bomb = this;
		TimeUntilExplode = 30f;

		Event.Run( PBEvent.Round.Bomb.Planted, this );
		Notification.Create( "Bomb has been planted!", 3 );
		Audio.Announce( "bomb_planted", Audio.Priority.Medium );
	}

	[ClientRpc]
	public void OnDisabled( Player defuser )
	{
		Disabled = true;
		Defuser = defuser;

		if ( Defuser != null )
			Event.Run( PBEvent.Round.Bomb.Defused, this );
		else
			Event.Run( PBEvent.Round.Bomb.Explode, this );
	}

	bool IUse.IsUsable( Entity user )
	{
		return !Disabled && user is Player player && player.Team == Team.Blue && Defuser == null && user.GroundEntity is WorldEntity;
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

		return !Disabled && player.Team == Team.Blue && (Defuser == null || Defuser == player);
	}

	void ILook.StartLook( Entity viewer )
	{
		if ( Defuser == null && viewer == Local.Pawn )
		{
			LookPanel = Local.Hud.AddChild<WeaponLookAt>();
			(LookPanel as WeaponLookAt).Text.Text = "Press E to defuse";
			(LookPanel as WeaponLookAt).Icon.SetTexture( "ui/weapons/bomb.png" );
		}
		else if ( Defuser == viewer )
		{
			LookPanel = Local.Hud.AddChild<BombDefuse>();
		}
	}

	void ILook.EndLook( Entity viewer )
	{
		LookPanel?.Delete();
	}

	private void OnDefuserChanged( Player oldDefuser, Player newDefuser )
	{
		if ( (Local.Pawn as Player).Looking != this )
			return;

		if ( newDefuser == (Local.Pawn as Player).CurrentPlayer && LookPanel is not BombDefuse )
		{
			LookPanel?.Delete();
			LookPanel = Local.Hud.AddChild<BombDefuse>();
		}
		else if ( newDefuser == null && LookPanel is BombDefuse )
		{
			LookPanel?.Delete();
			LookPanel = null;

			if ( !Local.Pawn.Alive() )
				return;

			LookPanel = Local.Hud.AddChild<WeaponLookAt>();
			(LookPanel as WeaponLookAt).Text.Text = "Press E to defuse";
			(LookPanel as WeaponLookAt).Icon.SetTexture( "ui/weapons/bomb.png" );
		}
	}
}
