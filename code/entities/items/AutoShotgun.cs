using Sandbox;

namespace Paintball;

[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
[Library( "pb_autoshotgun", Title = "AutoShotgun", Spawnable = true )]
public class AutoShotgun : ProjectileWeapon<BaseProjectile>
{
	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 );
		anim.SetParam( "aimat_weight", 1.0f );
	}
}

