using Sandbox;

namespace PaintBall
{
	public partial class Knife : Weapon
	{
		public override bool Automatic => true;
		public override int Bucket => 2;
		public override int ClipSize => 0;
		public override bool Droppable => false;
		public override string Icon => "ui/weapons/knife.png";
		public override bool IsMelee => true;
		public override string ModelPath => "models/rust_boneknife/rust_boneknife.vmdl";
		public override float PrimaryRate => 1.5f;
		public override float SecondaryRate => 0.75f;
		public override string ViewModelPath => "models/rust_boneknife/v_rust_boneknife.vmdl";

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/rust_boneknife/rust_boneknife.vmdl" );
		}

		public override bool CanReload()
		{
			return false;
		}

		public override void AttackPrimary()
		{
			base.AttackPrimary();

			MeleeAttack( 50f, 100f, 8f );
		}

		public override void AttackSecondary()
		{
			base.AttackSecondary();

			MeleeAttack( 100f, 50f, 16f );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 5 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		protected void MeleeAttack( float damage, float range, float radius )
		{
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			(Owner as AnimEntity).SetAnimBool( "b_attack", true );
			ShootEffects();

			var endPos = Owner.EyePos + Owner.EyeRot.Forward * range;

			var trace = Trace.Ray( Owner.EyePos, endPos )
				.UseHitboxes( true )
				.Ignore( this )
				.Ignore( Owner )
				.Radius( radius )
				.Run();

			if ( !trace.Hit )
				return;

			trace.Surface.DoBulletImpact( trace );

			if ( !IsServer )
				return;

			if ( trace.Entity is Player player && Owner is Player owner )
			{
				if ( player.Team == owner.Team )
					return;
			}

			using ( Prediction.Off() )
			{
				DamageInfo info = new DamageInfo()
					.UsingTraceResult( trace )
					.WithAttacker( Owner )
					.WithWeapon( this );

				info.Damage = damage;

				trace.Entity.TakeDamage( info );
			}
		}
	}
}
