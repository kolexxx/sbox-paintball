using Sandbox;

namespace PaintBall
{
	[Library( "pb_pistol", Title = "Pistol", Spawnable = true )]
	[Hammer.EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
	public partial class Pistol : ProjectileWeapon<BouncyProjectile>
	{
		public override int Bucket => 1;
		public override int ClipSize => 10;
		public override float Gravity => 10f;
		public override float PrimaryRate => 15f;
		public override float ProjectileRadius => 3f;
		public override float ReloadTime => 2.0f;
		public override float Speed => 1500f;
		public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
		public override bool UnlimitedAmmo => true;

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = ClipSize;

			SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 1 );
			anim.SetParam( "aimat_weight", 1.0f );
			anim.SetParam( "holdtype_handedness", 0 );
		}

		// TODO: This is bad
		public override bool CanPrimaryAttack()
		{
			return Input.Pressed( InputButton.Attack1 ) && base.CanPrimaryAttack();
		}
	}
}
