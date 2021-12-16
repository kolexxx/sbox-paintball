using Sandbox;
using System;

namespace PaintBall
{
	public abstract partial class Weapon : BaseWeapon
	{
		[Net, Predicted] public int AmmoClip { get; protected set; }
		[Net, Predicted] public bool IsReloading { get; protected set; }
		[Net, Predicted] public int ReserveAmmo { get; protected set; }
		[Net, Predicted] public TimeSince TimeSinceDeployed { get; protected set; }
		[Net, Predicted] public TimeSince TimeSinceReload { get; protected set; }
		public virtual bool Automatic => false;
		public virtual int Bucket => 0;
		public virtual int ClipSize => 20;
		public virtual string FireSound => "pbg";
		public virtual string FollowEffect => $"particles/{(Owner as Player)?.Team.GetString()}_glow.vpcf";
		public virtual float Gravity => 0f;
		public virtual string HitSound => "impact";
		public virtual string Icon => "ui/weapons/pistol.png";
		public Entity PreviousOwner { get; private set; }
		public virtual string ProjectileModel => $"models/{(Owner as Player)?.Team.GetString()}_ball/ball.vmdl";
		public virtual float ProjectileRadius => 3f;
		public virtual float ReloadTime => 5.0f;
		public virtual float Speed => 2000f;
		public virtual float Spread => 0f;
		public TimeSince TimeSinceDropped { get; private set; }
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
		public PickupTrigger PickupTrigger { get; protected set; }
		public virtual bool UnlimitedAmmo => false;

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

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			if ( !IsLocalPawn && IsActiveChild() )
			{
				CreateViewModel();

				if ( Local.Pawn is Player player && player.CurrentPlayer != Owner )
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

		public override bool CanReload()
		{
			if ( AmmoClip >= ClipSize || ( !UnlimitedAmmo && ReserveAmmo == 0 ) )
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

			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void Reload()
		{
			if ( IsReloading )
				return;

			TimeSinceReload = 0;
			IsReloading = true;

			(Owner as AnimEntity).SetAnimBool( "b_reload", true );

			ReloadEffects();
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

		[ClientRpc]
		protected virtual void ReloadEffects()
		{
			ViewModelEntity?.SetAnimBool( "reload", true );
		}

		[ClientRpc]
		protected void OnActiveStartClient()
		{
			if ( !IsLocalPawn && ViewModelEntity == null )
			{
				CreateViewModel();

				if ( (Local.Pawn as Player).CurrentPlayer != Owner )
					ViewModelEntity.EnableDrawing = false;

				ViewModelEntity?.SetAnimBool( "deploy", true );
			}
		}


		[ClientRpc]
		protected void OnActiveEndClient()
		{
			if ( !IsLocalPawn )
				DestroyViewModel();
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
				Owner = this,
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

		protected virtual int TakeAmmo( int ammo )
		{
			if ( UnlimitedAmmo )
				return ammo;

			int available = Math.Min( ReserveAmmo, ammo );
			ReserveAmmo -= available;

			return available;
		}
	}

}
