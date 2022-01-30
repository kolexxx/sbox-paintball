using Sandbox;

namespace Paintball;

public partial class Player : Sandbox.Player
{
	[Net] public Bombsite Bombsite { get; set; }
	[Net] public TimeSince TimeSinceSpawned { get; private set; }
	public ProjectileSimulator Projectiles { get; init; }
	public bool IsFrozen => Game.Current.State is MapSelectState || (Game.Current.State is GameplayState state && state.RoundState == RoundState.Freeze && !state.UntilStateEnds);
	public bool CanPlantBomb => Team == Team.Red && GroundEntity is WorldEntity && Bombsite.IsValid() && Game.Current.State is GameplayState;
	public bool IsDefusingBomb => Team == Team.Blue && Using is PlantedBomb;
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

	public override void Respawn()
	{
		TimeSinceSpawned = 0;
		ConsecutiveKills = 0;
		KillStreak = 0;

		RemoveCorpse();

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new CustomWalkController();

		Animator = new StandardPlayerAnimator();

		Camera = new FirstPersonCamera();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		
		RenderColor = Team.GetColor();
		Transmit = TransmitType.Always;	

		Game.Current.State.OnPlayerSpawned( this );

		LifeState = LifeState.Alive;
		Health = 100;
		Velocity = Vector3.Zero;
		WaterLevel.Clear();
		TimeSinceSpawned = 0f;

		CreateHull();
		ResetInterpolation();
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
		if ( other is Weapon weapon && weapon.PreviousOwner == this && weapon.TimeSinceDropped <= 2f )
			return;

		if ( other is PickupTrigger )
		{
			StartTouch( other.Parent );

			return;
		}

		Inventory.Pickup( other );		
	}

	public void Reset()
	{
		Inventory.DeleteContents();

		Client.SetInt( "deaths", 0 );
		Client.SetInt( "kills", 0 );

		LastAttacker = null;
		LastDamageInfo = default;
		ConsecutiveKills = 0;
		KillStreak = 0;
		Money = 1000;
		LifeState = LifeState.Respawnable;
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );
		Transmit = TransmitType.Always;
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
					dropped.PhysicsGroup.Velocity = Velocity + (EyeRot.Forward + EyeRot.Up) * 300;

					SwitchToBestWeapon();
				}
			}
		}
	}
}
