using Sandbox;

namespace Paintball;

public partial class Player : Sandbox.Player
{
	[Net] public Bombsite Bombsite { get; set; }
	[Net] public TimeSince TimeSinceSpawned { get; private set; }
	public ProjectileSimulator Projectiles { get; init; }
	public bool IsFrozen
	{
		get
		{
			if ( Game.Current.State is MapSelectState )
				return true;

			if ( Game.Current.State is not GameplayState state )
				return false;

			return state.RoundState == RoundState.Freeze && !state.UntilStateEnds;
		}
	}
	public bool CanPlantBomb
	{
		get
		{
			if ( Game.Current.State is not GameplayState || !GameplayState.BombEnabled )
				return false;

			if ( GroundEntity is not WorldEntity || Bombsite == null )
				return false;

			return true;
		}
	}
	public bool IsDefusingBomb => Using is PlantedBomb;
	public bool IsPlantingBomb { get; set; }

	public new Inventory Inventory
	{
		get => base.Inventory as Inventory;
		private init => base.Inventory = value;
	}

	public new CustomWalkController Controller
	{
		get => base.Controller as CustomWalkController;
		private set => base.Controller = value;
	}

	public Player()
	{
		Inventory = new Inventory( this );
		Projectiles = new( this );
		EnableTouch = true;
		EnableShadowInFirstPerson = true;

		LifeState = LifeState.Respawnable;
	}

	public async void Respawn( float delay )
	{
		await GameTask.DelaySeconds( delay );

		if ( this.IsValid() && Game.Current.State is WaitingForPlayersState )
			Respawn();
	}

	public override void Respawn()
	{
		if ( Team == Team.None )
			return;

		TimeSinceSpawned = 0;
		ConsecutiveKills = 0;
		KillStreak = 0;

		Controller = new CustomWalkController();

		Animator = new StandardPlayerAnimator();

		Camera = new FirstPersonCamera();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.ForEach( ( entity ) =>
		{
			entity.EnableDrawing = true;
		} );

		Transmit = TransmitType.Always;

		Game.Current.State.OnPlayerSpawned( this );

		LifeState = LifeState.Alive;
		Health = 100;
		Velocity = Vector3.Zero;
		WaterLevel.Clear();
		TimeSinceSpawned = 0f;

		SwitchToBestWeapon();
		CreateHull();
		ResetInterpolation();
		RemoveCorpse();
	}

	public override void Simulate( Client cl )
	{
		Projectiles.Simulate();

		var controller = GetActiveController();

		SimulateActiveChild( cl, ActiveChild );

		if ( Input.ActiveChild != null )
			ActiveChild = Input.ActiveChild;

		if ( this.Alive() )
		{
			TickPlayerUse();
			TickPlayerDrop();
		}
		else
		{
			TickPlayerChangeSpectateCamera();
		}

		TickPlayerLook();

		controller?.Simulate( cl, this, GetActiveAnimator() );
	}

	public override void StartTouch( Entity other )
	{
		if ( other is Carriable weapon && weapon.PreviousOwner == this && weapon.TimeSinceDropped <= 2f )
			return;

		if ( other is PickupTrigger )
		{
			StartTouch( other.Parent );

			return;
		}

		if ( IsServer )
			Inventory.Pickup( other );
	}

	public void Reset()
	{
		Inventory.DeleteContents();

		Client.SetInt( "deaths", 0 );
		Client.SetInt( "kills", 0 );

		LastAttacker = null;
		LastDamageInfo = default;
		LastWeaponInfo = null;
		ConsecutiveKills = 0;
		KillStreak = 0;
		Money = 1000;
		LifeState = LifeState.Respawnable;
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );
		SetModel( "models/citizen/citizen.vmdl" );
		AttachClothing( "models/helmet/paintballhelmet.vmdl" );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		CreateHull();
	}

	private void TickPlayerDrop()
	{
		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();

			if ( dropped != null )
			{
				if ( dropped.PhysicsGroup != null )
				{
					dropped.PhysicsGroup.Velocity = Velocity + (EyeRotation.Forward + EyeRotation.Up) * 300;

					SwitchToBestWeapon();
				}
			}
		}
	}
}
