using Sandbox;

namespace PaintBall
{
	[Library( "pb_shotgun", Title = "Shotgun", Spawnable = true )]
	[Hammer.EditorModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" )]
	public partial class Shotgun : ProjectileWeapon<Projectile>
	{
		public override int Bucket => 0;
		public virtual int BulletsPerFire => 4;
		public override int ClipSize => 5;
		public override float Gravity => 7f;
		public override string Icon => "ui/weapons/shotgun.png";
		public override float PrimaryRate => 1f;
		public override float ProjectileRadius => 3f;
		public override float ReloadTime => 0.7f;
		public override float Speed => 2500f;
		public override float Spread => 0.05f;
		public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
		private bool _attackedDuringReload = false;

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = ClipSize;
			ReserveAmmo = 10;

			SetModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
		}

		public override void ActiveStart( Entity entity )
		{
			base.ActiveStart( entity );

			_attackedDuringReload = false;
			TimeSinceReload = 0f;
		}

		public override bool CanReload()
		{
			if ( AmmoClip >= ClipSize || (!UnlimitedAmmo && ReserveAmmo == 0) )
				return false;

			if ( !Owner.IsValid() || !Input.Down( InputButton.Reload ) )
				return false;

			var rate = PrimaryRate;
			if ( rate <= 0 )
				return true;

			return TimeSincePrimaryAttack > (1 / rate);
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

				for ( int i = 0; i < BulletsPerFire; i++ )
					FireProjectile();
			}
		}

		public override void Simulate( Client owner )
		{
			if ( TimeSinceDeployed < 0.6f )
				return;

			if ( !IsReloading )
			{
				if ( CanReload() )
					Reload();

				if ( !Owner.IsValid() )
					return;

				if ( CanPrimaryAttack() )
				{
					TimeSincePrimaryAttack = 0;
					AttackPrimary();
				}

				if ( !Owner.IsValid() )
					return;

				if ( CanSecondaryAttack() )
				{
					TimeSinceSecondaryAttack = 0;
					AttackSecondary();
				}
			}
			else if ( Input.Pressed( InputButton.Attack1 ) )
			{
				_attackedDuringReload = true;
			}

			if ( IsReloading && TimeSinceReload > ReloadTime )
				OnReloadFinish();
		}

		public override void OnReloadFinish()
		{
			IsReloading = false;

			TimeSincePrimaryAttack = 0;

			AmmoClip += TakeAmmo( 1 );

			if ( !_attackedDuringReload && AmmoClip < ClipSize && (UnlimitedAmmo || ReserveAmmo != 0) )
				Reload();
			else
				FinishReload();

			_attackedDuringReload = false;
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 3 );
		}

		[ClientRpc]
		public void FinishReload()
		{
			ViewModelEntity?.SetAnimBool( "reload_finished", true );
		}
	}
}
