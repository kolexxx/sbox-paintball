using Sandbox;

namespace Paintball;

[Hammer.Skip]
public abstract partial class ProjectileWeapon<T> : Weapon where T : BaseProjectile, new()
{
	public virtual int BulletsPerFire => 1;
	public virtual string FollowEffect => $"particles/{Owner.Team.GetString()}_glow.vpcf";
	public virtual float Gravity => 0f;
	public virtual string HitSound => "impact";
	public virtual string ProjectileModel => $"models/paintball/paintball.vmdl";
	public virtual float ProjectileRadius => 3f;
	public virtual float ProjectileScale => 0.25f;
	public virtual float Speed => 2000f;
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

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
		PlaySound( FireSound );

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
			Scale = ProjectileScale,
			Radius = ProjectileRadius,
			Gravity = Gravity,
			Simulator = owner.Projectiles,
			// ModelPath = ProjectileModel,
			Rotation = owner.EyeRot,
			Origin = this
		};

		var forward = owner.EyeRot.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Spread * 0.25f;
		forward = forward.Normal;

		var position = owner.EyePos;

		var velocity = forward * Speed;

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
					Scale = ProjectileScale,
					Radius = ProjectileRadius,
					Gravity = Gravity,
					Simulator = owner.Projectiles,
					// ModelPath = ProjectileModel,
					Rotation = owner.EyeRot,
					Origin = this
				};

				var angles = owner.EyeRot.Angles();
				angles.pitch += pitch;
				angles.yaw += yaw;

				var forward = Rotation.From( angles ).Forward;
				forward = forward.Normal;

				var position = owner.EyePos;

				var velocity = forward * Speed;

				projectile.Initialize( position, velocity );
			}
		}
	}
}
