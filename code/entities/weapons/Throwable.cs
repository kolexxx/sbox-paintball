using Sandbox;

namespace PaintBall
{
	[Library( "pb_grenade", Title = "grenade", Spawnable = true )]
	[Hammer.EditorModel( "models/grenade/grenade.vmdl" )]
	public partial class Throwable : Weapon
	{
		public override int Bucket => 3;
		public override int ClipSize => 1;
		public override string FireSound => "";
		public override string Icon => "ui/weapons/grenade.png";
		public override float PrimaryRate => 15f;
		public override float ReloadTime => 2.0f;
		public override string ViewModelPath => "models/grenade/v_grenade.vmdl";

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = ClipSize;

			SetModel( "models/grenade/grenade.vmdl" );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 5 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		public override void AttackPrimary()
		{
			base.AttackPrimary();

			if ( !IsServer )
				return;

			using ( Prediction.Off() )
			{
				var ent = new Grenade();
				ent.Rotation = Owner.EyeRot;
				ent.Position = Owner.EyePos + (Owner.EyeRot.Forward * 40);
				ent.Velocity = Owner.EyeRot.Forward * 1000 + Vector3.Up * 100;
				ent.Callback = OnProjectileHit;
				ent.Owner = Owner;

				(Owner as Player).SwitchToBestWeapon();
				Delete();
			}
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
