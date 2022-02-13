using Sandbox;

namespace Paintball;

[Library( "pb_pistol", Title = "Pistol", Icon = "ui/weapons/pistol.png", Spawnable = true )]
[Hammer.EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
public partial class Pistol : ProjectileWeapon<BouncyProjectile>
{
	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 1 );
		anim.SetParam( "aimat_weight", 1.0f );
		anim.SetParam( "holdtype_handedness", 0 );
	}
}
