using Sandbox;

namespace Paintball;

[Hammer.EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
[Library( "pb_pistol", Title = "Pistol")]
public partial class Pistol : ProjectileWeapon<BouncyProjectile>
{
	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 1 );
		anim.SetParam( "aimat_weight", 1.0f );
		anim.SetParam( "holdtype_handedness", 0 );
	}
}
