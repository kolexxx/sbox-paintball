using Sandbox;

namespace PaintBall
{
	[Library( "pb_shotgun", Title = "Shotgun", Spawnable = true )]
	public partial class Shotgun : Weapon
	{
		public virtual int BulletsPerFire => 4;
		public override int ClipSize => 5;
		public override float Gravity => 7f;
		public override string Icon => "ui/weapons/shotgun.png";
		public override float PrimaryRate => 1f;
		public override float ProjectileRadius => 3f;
		public override float ReloadTime => 0.5f;
		public override float Speed => 3000f;
		public override float Spread => 0.05f;
		public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = ClipSize;
			SetModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
		}

		public override void AttackPrimary()
		{
			if ( AmmoClip == 0 )
			{
				Reload();
				return;
			}

			Rand.SetSeed( Time.Tick );

			TimeSincePrimaryAttack = 0;
			AmmoClip--;

			(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

			ShootEffects();
			PlaySound( FireSound );

			if ( Prediction.FirstTime )
			{
				Rand.SetSeed( Time.Tick );
				for ( int i = 0; i < BulletsPerFire; i++ )
				{ 
					FireProjectile();
				}
			}
		}

		public override void OnReloadFinish()
		{
			IsReloading = false;

			TimeSincePrimaryAttack = 0;

			AmmoClip++;
			if(AmmoClip < ClipSize )
			{
				Reload();
			} else
			{
				FinishReload();
			}
		}

		[ClientRpc]
		public void FinishReload()
		{
			ViewModelEntity?.SetAnimBool( "reload_finished", true );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 3 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
	}
}
