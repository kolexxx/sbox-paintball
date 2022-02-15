using Sandbox;

namespace Paintball;

[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
[Library( "pb_smg", Title = "SMG", Spawnable = true )]
public partial class SMG : ProjectileWeapon<BaseProjectile>
{
	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 );
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
