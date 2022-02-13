using Sandbox;
using System;
using System.ComponentModel;

namespace Paintball;

public enum FireMode
{
	Automatic,
	Semi,
	Burst
}

[Library( "pwpn" ), AutoGenerate]
public partial class ProjectileWeaponInfo : CarriableInfo
{
	[Property, Category( "VFX" ), ResourceType( "vpcf" )] public string FollowEffect { get; set; }
	[Property, Category( "Sounds" )] public string FireSound { get; set; }
	[Property, Category( "Models" ), ResourceType( "vmdl" )] public string ProjectileModel { get; set; } = "models/paintball/paintball.vmdl";
	[Property, Category( "Stats" )] public int ClipSize { get; set; }
	[Property, Category( "Stats" )] public int BulletsPerFire { get; set; } = 1;
	[Property, Category( "Stats" )] public FireMode FireMode { get; set; }
	[Property, Category( "Stats" )] public float PrimaryRate { get; set; } = 5f;
	[Property, Category( "Stats" )] public float ProjectileGravity { get; set; }
	[Property, Category( "Stats" )] public float ProjectileRadius { get; set; } = 3f;
	[Property, Category( "Stats" )] public float ProjectileScale { get; set; } = 0.25f;
	[Property, Category( "Stats" )] public float ProjectileSpeed { get; set; } = 2000f;
	[Property, Category( "Stats" )] public float ReloadTime { get; set; } = 5f;
	[Property, Category( "Stats" )] public int ReserveAmmo { get; set; }
	[Property, Category( "Stats" )] public float SecondaryRate { get; set; }
	[Property, Category( "Stats" )] public float Spread { get; set; } = 0f;
}

[Hammer.Skip]
public abstract partial class ProjectileWeapon<T> : Carriable where T : BaseProjectile, new()
{
	[Net, Predicted] public bool IsReloading { get; protected set; }
	[Net, Predicted] public int ReserveAmmo { get; protected set; }
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; protected set; }
	[Net, Predicted] public TimeSince TimeSinceSecondaryAttack { get; protected set; }
	[Net, Predicted] public TimeSince TimeSinceReload { get; protected set; }
	public virtual string FollowEffect => $"particles/{Owner.Team.GetTag()}_glow.vpcf";
	public new ProjectileWeaponInfo Info
	{
		get => base.Info as ProjectileWeaponInfo;
		set => base.Info = value;
	}
	public bool UnlimitedAmmo { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = Info.ClipSize;
		ReserveAmmo = Info.ReserveAmmo;
	}

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( !IsReloading )
		{
			if ( CanReload() )
			{
				Reload();
			}

			//
			// Reload could have changed our owner
			//
			if ( !Owner.IsValid() )
				return;

			if ( CanPrimaryAttack() )
			{
				using ( LagCompensation() )
				{
					TimeSincePrimaryAttack = 0;
					AttackPrimary();
				}
			}

			//
			// AttackPrimary could have changed our owner
			//
			if ( !Owner.IsValid() )
				return;

			if ( CanSecondaryAttack() )
			{
				using ( LagCompensation() )
				{
					TimeSinceSecondaryAttack = 0;
					AttackSecondary();
				}
			}
		}
		else if ( IsReloading && TimeSinceReload > Info.ReloadTime )
			OnReloadFinish();
	}

	public virtual bool CanPrimaryAttack()
	{
		if ( Owner.IsFrozen )
			return false;

		if ( Info.FireMode == FireMode.Semi && !Input.Pressed( InputButton.Attack1 ) )
			return false;
		else if ( Info.FireMode == FireMode.Automatic && !Input.Down( InputButton.Attack1 ) )
			return false;

		var rate = Info.PrimaryRate;
		if ( rate <= 0 )
			return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual bool CanSecondaryAttack()
	{
		if ( Owner.IsFrozen )
			return false;

		if ( !Input.Pressed( InputButton.Attack2 ) )
			return false;

		var rate = Info.SecondaryRate;
		if ( rate <= 0 )
			return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	public virtual void AttackPrimary()
	{
		if ( AmmoClip == 0 )
		{
			if ( !UnlimitedAmmo && ReserveAmmo == 0 )
			{
				// Play dryfire sound
				return;
			}

			Reload();

			return;
		}

		AmmoClip--;

		Owner.SetAnimBool( "b_attack", true );

		ShootEffects();
		PlaySound( Info.FireSound );

		if ( Prediction.FirstTime )
		{
			Rand.SetSeed( Time.Tick );

			if ( Info.BulletsPerFire == 1 )
				FireProjectile();
			else
				FireProjectilesInPatern();
		}
	}

	public virtual void AttackSecondary() { }

	public virtual bool CanReload()
	{
		if ( AmmoClip >= Info.ClipSize || (!UnlimitedAmmo && ReserveAmmo == 0) )
			return false;

		if ( !Owner.IsValid() || !Input.Down( InputButton.Reload ) )
			return false;

		return true;
	}

	public virtual void Reload()
	{
		if ( IsReloading )
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		Owner.SetAnimBool( "b_reload", true );

		ReloadEffects();
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
		AmmoClip += TakeAmmo( Info.ClipSize - AmmoClip );
	}

	public override void Reset()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
		TimeSinceReload = 0;
		AmmoClip = Info.ClipSize;
		ReserveAmmo = Info.ReserveAmmo;

		base.Reset();
	}

	protected void FireProjectile()
	{
		if ( Owner is not Player owner )
			return;

		var projectile = new T()
		{
			Owner = owner,
			Team = owner.Team,
			// FollowEffect = FollowEffect,
			// HitSound = HitSound,
			Scale = Info.ProjectileScale,
			Radius = Info.ProjectileRadius,
			Gravity = Info.ProjectileGravity,
			Simulator = owner.Projectiles,
			// ModelPath = ProjectileModel,
			Rotation = owner.EyeRotation,
			Origin = this
		};

		var forward = owner.EyeRotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Info.Spread * 0.25f;
		forward = forward.Normal;

		var position = owner.EyePosition;

		var velocity = forward * Info.ProjectileSpeed;

		projectile.Initialize( position, velocity );
	}

	protected void FireProjectilesInPatern()
	{
		if ( Owner is not Player owner )
			return;

		for ( float pitch = -1f; pitch <= 1f; pitch += 1f )
		{
			for ( float yaw = -1f; yaw <= 1f; yaw += 1f )
			{
				var projectile = new T()
				{
					Owner = owner,
					Team = owner.Team,
					// FollowEffect = FollowEffect,
					// HitSound = HitSound,
					Scale = Info.ProjectileScale,
					Radius = Info.ProjectileRadius,
					Gravity = Info.ProjectileGravity,
					Simulator = owner.Projectiles,
					// ModelPath = ProjectileModel,
					Rotation = owner.EyeRotation,
					Origin = this
				};

				var angles = owner.EyeRotation.Angles();
				angles.pitch += pitch;
				angles.yaw += yaw;

				var forward = Rotation.From( angles ).Forward;
				forward = forward.Normal;

				var position = owner.EyePosition;

				var velocity = forward * Info.ProjectileSpeed;

				projectile.Initialize( position, velocity );
			}
		}
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		// Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		if ( IsLocalPawn )
			_ = new Sandbox.ScreenShake.Perlin( 1f, 0.2f, 0.8f );

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	[ClientRpc]
	protected virtual void ReloadEffects()
	{
		ViewModelEntity?.SetAnimBool( "reload", true );
	}

	protected int TakeAmmo( int ammo )
	{
		if ( UnlimitedAmmo )
			return ammo;

		int available = Math.Min( ReserveAmmo, ammo );
		ReserveAmmo -= available;

		return available;
	}
}
