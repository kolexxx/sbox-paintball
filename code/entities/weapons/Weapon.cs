using Paintball.UI;
using Sandbox;
using Sandbox.UI;
using System;

namespace Paintball;

public enum SlotType : byte
{
	Primary = 0,
	Secondary = 1,
	Melee = 2,
	Utility = 3,
	Deployable = 4,
}

[Hammer.Skip]
public abstract partial class Weapon : BaseWeapon, IUse, ILook
{
	[Net, Predicted] public int AmmoClip { get; set; }
	[Net, Predicted] public bool IsReloading { get; protected set; }
	[Net, Predicted] public int ReserveAmmo { get; protected set; }
	[Net, Predicted] public TimeSince TimeSinceDeployed { get; protected set; }
	[Net, Predicted] public TimeSince TimeSinceReload { get; protected set; }
	public virtual bool Automatic => false;
	public virtual int ClipSize => 20;
	public virtual string CrosshairClass => "standard";
	public virtual bool Droppable => true;
	public virtual Team ExclusiveFor => Team.None;
	public virtual string FireSound => "pbg";
	public virtual string Icon => "ui/weapons/pistol.png";
	public virtual bool IsMelee => false;
	public Panel LookPanel { get; set; }
	public virtual string ModelPath => "";
	public virtual float MovementSpeedMultiplier => 1;
	public PickupTrigger PickupTrigger { get; protected set; }
	public Entity PreviousOwner { get; private set; }
	public virtual float ReloadTime => 5f;
	public virtual SlotType Slot => SlotType.Primary;
	public virtual float Spread => 0f;
	public TimeSince TimeSinceDropped { get; private set; }
	public virtual bool UnlimitedAmmo => false;

	public new Player Owner
	{
		get => base.Owner as Player;
		set => base.Owner = value;
	}

	public Weapon() { }

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position
		};

		PickupTrigger.PhysicsBody.EnableAutoSleeping = false;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		if ( Local.Pawn is not Player player )
			return;

		if ( !IsLocalPawn && IsActiveChild() )
		{
			CreateViewModel();
			CreateHudElements();

			if ( player.Alive() || player.CurrentPlayer != Owner )
				ViewModelEntity.EnableDrawing = false;
		}
	}

	public override void ActiveStart( Entity entity )
	{
		base.ActiveStart( entity );

		TimeSinceDeployed = 0;
		IsReloading = false;

		if ( IsServer )
			OnActiveStartClient( To.Everyone );
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		if ( IsServer )
			OnActiveEndClient( To.Everyone );
	}

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( !IsReloading )
			base.Simulate( owner );

		if ( IsReloading && TimeSinceReload > ReloadTime )
			OnReloadFinish();
	}

	public override bool CanPrimaryAttack()
	{
		if ( Owner.IsFrozen )
			return false;

		if ( Automatic == false && !Input.Pressed( InputButton.Attack1 ) )
			return false;
		else if ( Automatic == true && !Input.Down( InputButton.Attack1 ) )
			return false;

		var rate = PrimaryRate;
		if ( rate <= 0 )
			return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public override bool CanSecondaryAttack()
	{
		if ( Owner.IsFrozen )
			return false;

		if ( !Input.Pressed( InputButton.Attack2 ) )
			return false;

		var rate = SecondaryRate;
		if ( rate <= 0 )
			return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	public override void Reload()
	{
		if ( IsReloading )
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		Owner.SetAnimBool( "b_reload", true );

		ReloadEffects();
	}

	public override bool CanReload()
	{
		if ( AmmoClip >= ClipSize || (!UnlimitedAmmo && ReserveAmmo == 0) )
			return false;

		return base.CanReload();
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};

		ViewModelEntity.FieldOfView = 70;
		ViewModelEntity.SetModel( ViewModelPath );
	}

	public override void CreateHudElements()
	{
		if ( Local.Hud == null ) return;

		CrosshairPanel = new Crosshair
		{
			Parent = Local.Hud,
			TargetWeapon = this
		};

		CrosshairPanel.AddClass( CrosshairClass );
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
		AmmoClip += TakeAmmo( ClipSize - AmmoClip );
	}

	public override void OnCarryStart( Entity carrier )
	{
		base.OnCarryStart( carrier );

		if ( PickupTrigger.IsValid() )
			PickupTrigger.EnableTouch = false;

		PreviousOwner = Owner;
	}

	public override void OnCarryDrop( Entity dropper )
	{
		base.OnCarryDrop( dropper );

		if ( PickupTrigger.IsValid() )
			PickupTrigger.EnableTouch = true;

		TimeSinceDropped = 0f;

		OnActiveEndClient( To.Everyone );
	}

	public void Remove()
	{
		PhysicsGroup?.Wake();
		Delete();
	}

	public void DealDamage( Entity entity, Entity attacker, Vector3 position, Vector3 force, int hitbox )
	{
		var info = new DamageInfo()
			.WithAttacker( attacker )
			.WithWeapon( this )
			.WithPosition( position )
			.WithForce( force )
			.WithHitbox( hitbox );

		info.Damage = float.MaxValue;

		entity.TakeDamage( info );
	}

	protected int TakeAmmo( int ammo )
	{
		if ( UnlimitedAmmo )
			return ammo;

		int available = Math.Min( ReserveAmmo, ammo );
		ReserveAmmo -= available;

		return available;
	}

	public void Reset()
	{
		AmmoClip = ClipSize;
		ReserveAmmo = 2 * ClipSize;
		TimeSinceDeployed = 0;
		TimeSincePrimaryAttack = 0;
		TimeSinceDropped = 0;
		TimeSinceReload = 0;
		TimeSinceSecondaryAttack = 0;
		IsReloading = false;

		ClientReset();
	}

	#region rpc
	[ClientRpc]
	protected void ClientReset()
	{
		SetAnimBool( "idle", true );
	}

	[ClientRpc]
	protected void OnActiveStartClient()
	{
		if ( IsLocalPawn || ViewModelEntity != null )
			return;

		CreateViewModel();

		if ( (Local.Pawn as Player).CurrentPlayer != Owner )
			ViewModelEntity.EnableDrawing = false;

		ViewModelEntity?.SetAnimBool( "deploy", true );
	}

	[ClientRpc]
	protected void OnActiveEndClient()
	{
		if ( IsLocalPawn )
			return;

		DestroyHudElements();
		DestroyViewModel();
	}

	[ClientRpc]
	protected virtual void ReloadEffects()
	{
		ViewModelEntity?.SetAnimBool( "reload", true );
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Host.AssertClient();

		// Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		if ( IsLocalPawn )
			_ = new Sandbox.ScreenShake.Perlin( 1f, 0.2f, 0.8f );

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	[ClientRpc]
	protected virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimBool( "reload", true );
	}
	#endregion

	bool IUse.OnUse( Entity user )
	{
		if ( IsServer && user is Player player )
			player.Inventory.Swap( this );

		return false;
	}

	bool IUse.IsUsable( Entity user )
	{
		return Owner == null && user is Player;
	}

	bool ILook.IsLookable( Entity viewer )
	{
		return viewer is Player player && (ExclusiveFor == Team.None || player.Team == ExclusiveFor);
	}

	void ILook.StartLook( Entity viewer )
	{
		if ( viewer != Local.Pawn )
			return;

		LookPanel = Local.Hud.AddChild<WeaponLookAt>();
		(LookPanel as WeaponLookAt).Icon.SetTexture( Icon );
	}

	void ILook.EndLook( Entity viewer )
	{
		LookPanel?.Delete();
	}
}
