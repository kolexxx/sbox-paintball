using Sandbox;

namespace PaintBall
{
	public abstract partial class Weapon : BaseWeapon
	{
		[Net, Predicted] public int AmmoClip { get; set; }
		[Net, Predicted] public bool IsReloading { get; set; }
		[Net, Predicted] public TimeSince TimeSinceDeployed { get; set; }
		[Net, Predicted] public TimeSince TimeSinceReload { get; set; }
		public virtual bool Automatic => false;
		public virtual int Bucket => 0;
		public virtual int ClipSize => 20;
		public virtual string FireSound => "pbg";
		public virtual string FollowEffect => $"particles/{(Owner as Player).Team.GetString()}_glow.vpcf";
		public virtual float Gravity => 0f;
		public virtual string HitSound => "impact";
		public virtual string Icon => "ui/weapons/pistol.png";
		public virtual string ProjectileModel => $"models/{(Owner as Player).Team.GetString()}_ball/ball.vmdl";
		public virtual float ProjectileRadius => 3f;
		public virtual float ReloadTime => 5.0f;
		public virtual float Speed => 2000f;
		public virtual float Spread => 0f;
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
		public PickupTrigger PickupTrigger { get; protected set; }

		public Weapon()
		{
			EnableShadowInFirstPerson = false;
		}

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

		public override void ActiveStart( Entity entity )
		{
			base.ActiveStart( entity );

			TimeSinceDeployed = 0;
			IsReloading = false;
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

		public override void AttackPrimary()
		{
			if ( AmmoClip == 0 )
			{
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

		public override bool CanPrimaryAttack()
		{
			if ( Game.Instance.CurrentGameState.FreezeTime <= 5f )
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

			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void Reload()
		{
			if ( IsReloading )
				return;

			if ( AmmoClip >= ClipSize )
				return;

			TimeSinceReload = 0;
			IsReloading = true;

			(Owner as AnimEntity).SetAnimBool( "b_reload", true );

			ReloadEffects();
		}

		public virtual void OnReloadFinish()
		{
			IsReloading = false;
			AmmoClip = ClipSize;
		}

		public override void OnCarryStart( Entity carrier )
		{
			base.OnCarryStart( carrier );

			if ( PickupTrigger.IsValid() )	
				PickupTrigger.EnableTouch = false;

			foreach(var entity in Owner.Children )
			{
				if ( entity is not Weapon weapon || this == weapon )
					continue;

				if ( Bucket > weapon.Bucket )
					return;
			}

			Owner.ActiveChild = this;
		}

		public override void OnCarryDrop( Entity dropper )
		{
			base.OnCarryDrop( dropper );

			if ( PickupTrigger.IsValid() )	
				PickupTrigger.EnableTouch = true;
		}

		public void Remove()
		{
			PhysicsGroup?.Wake();
			Delete();
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
			{
				_ = new Sandbox.ScreenShake.Perlin( 1f, 0.2f, 0.8f );
			}

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "fire" );
		}

		[ClientRpc]
		protected virtual void StartReloadEffects()
		{
			ViewModelEntity?.SetAnimBool( "reload", true );
		}

		protected void FireProjectile()
		{
			if ( Owner is not Player owner )
				return;

			var projectile = new Projectile()
			{
				Team = owner.Team,
				FollowEffect = FollowEffect,
				HitSound = HitSound,
				IgnoreTag = $"{owner.Team.GetString()}player",
				Scale = 0.2f,
				Radius = ProjectileRadius,
				Gravity = Gravity,
				Simulator = owner.Projectiles,
				Model = ProjectileModel
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
				DealDamage( entity, projectile.Position, projectile.Velocity * 0.1f, hitbox );
		}

		protected void DealDamage( Entity entity, Vector3 position, Vector3 force, int hitbox )
		{
			var info = new DamageInfo()
				.WithAttacker( Owner )
				.WithWeapon( this )
				.WithPosition( position )
				.WithForce( force )
				.WithHitbox( hitbox );

			info.Damage = float.MaxValue;

			entity.TakeDamage( info );
		}
	}

}
