using Sandbox;

namespace PaintBall
{
	[Library( "pb_grenade", Title = "grenade", Spawnable = true )]
	[Hammer.EditorModel( "models/grenade/grenade.vmdl" )]
	public partial class Throwable : ProjectileWeapon<Grenade>
	{
		public override int Bucket => 3;
		public override int ClipSize => 1;
		public override string FireSound => "";
		public override string FollowEffect => "";
		public override float Gravity => 10f;
		public override string Icon => "ui/weapons/grenade.png";
		public override float PrimaryRate => 15f;
		public override string ProjectileModel => "models/grenade/grenade_spent.vmdl";
		public override float ProjectileRadius => 3f;
		public override float ProjectileScale => 1f;
		public override float ReloadTime => 2.0f;
		public override float Speed => 1500f;
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

			TimeSincePrimaryAttack = 0;

			if ( !IsServer )
				return;

			if ( AmmoClip == 0 )
			{
				(Owner as Player).SwitchToBestWeapon();
				Delete();			
			}
		}
	}
}
