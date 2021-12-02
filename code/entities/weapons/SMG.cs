using Sandbox;

namespace PaintBall
{
	[Library( "pb_smg", Title = "SMG", Spawnable = true )]
	public partial class SMG : Weapon
	{
		public override int ClipSize => 20;
		public override float Gravity => 7f;
		public override string Icon => "ui/weapons/smg.png";
		public override float PrimaryRate => 6f;
		public override float ProjectileRadius => 3f;
		public override float ReloadTime => 3f;
		public override float Speed => 2000f;
		public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = ClipSize;
			SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 2 );
			anim.SetParam( "aimat_weight", 1.0f );
		}
	}
}
