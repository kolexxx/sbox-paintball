using Sandbox;

namespace PaintBall
{
	public abstract partial class ProjectileWeapon<T> : Weapon where T : Projectile, new()
	{
		public virtual string FollowEffect => $"particles/{(Owner as Player)?.Team.GetString()}_glow.vpcf";
		public virtual float Gravity => 0f;
		public virtual string HitSound => "impact";
		public virtual string ProjectileModel => $"models/{(Owner as Player)?.Team.GetString()}_ball/ball.vmdl";
		public virtual float ProjectileRadius => 4f;
		public virtual float ProjectileScale => 0.25f;
		public virtual float Speed => 2000f;
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

		public override void AttackPrimary()
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

			TimeSincePrimaryAttack = 0;
			AmmoClip--;

			(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

			ShootEffects();
			PlaySound( FireSound );

			if ( Prediction.FirstTime )
			{
				Rand.SetSeed( Time.Tick );
				FireProjectile();
			}
		}

		protected void FireProjectile()
		{
			if ( Owner is not Player owner )
				return;

			//if ( IsLocalPawn )
			//Log.Info( owner.Projectiles.IsValid() );

			var projectile = new T()
			{
				Owner = this,
				Team = owner.Team,
				FollowEffect = FollowEffect,
				HitSound = HitSound,
				IgnoreTag = $"{owner.Team.GetString()}player",
				Scale = ProjectileScale,
				Radius = ProjectileRadius,
				Gravity = Gravity,
				Simulator = owner.Projectiles,
				Model = ProjectileModel,
				Rotation = owner.EyeRot
			};

			var forward = owner.EyeRot.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * Spread * 0.25f;
			forward = forward.Normal;

			var position = owner.EyePos;

			var velocity = forward * Speed;

			projectile.Initialize( position, velocity, OnProjectileHit );
		}

		protected virtual void OnProjectileHit( Projectile projectile, Entity entity, int hitbox )
		{
			if ( IsServer && entity.IsValid() )
				DealDamage( entity, projectile.Owner, projectile.Position, projectile.Velocity * 0.1f, hitbox );
		}

		protected void DealDamage( Entity entity, Entity attacker, Vector3 position, Vector3 force, int hitbox )
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
	}
}
