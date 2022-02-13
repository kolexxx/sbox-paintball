using Sandbox;
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
	public virtual int BulletsPerFire => 1;
	public virtual string FollowEffect => $"particles/{Owner.Team.GetTag()}_glow.vpcf";
	public virtual string HitSound => "impact";
	public new ProjectileWeaponInfo Info
	{
		get => base.Info as ProjectileWeaponInfo;
		set => base.Info = value;
	}
	public virtual string ProjectileModel => "models/paintball/paintball.vmdl";
	public virtual float ProjectileGravity => 0f;
	public virtual float ProjectileRadius => 3f;
	public virtual float ProjectileScale => 0.25f;
	public virtual float ProjectileSpeed => 2000f;
	public virtual float Spread => 0f;
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = Info.ClipSize;
		ReserveAmmo = Info.ReserveAmmo;
	}

	public override bool CanPrimaryAttack()
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

	public override void AttackPrimary()
	{
		base.AttackPrimary();

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

			if ( BulletsPerFire == 1 )
				FireProjectile();
			else
				FireProjectilesInPatern();
		}
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
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Spread * 0.25f;
		forward = forward.Normal;

		var position = owner.EyePosition;

		var velocity = forward * ProjectileSpeed;

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

				var velocity = forward * ProjectileSpeed;

				projectile.Initialize( position, velocity );
			}
		}
	}
}
