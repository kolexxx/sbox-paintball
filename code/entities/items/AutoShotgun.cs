using Sandbox;

namespace Paintball;

[Library( "pb_autoshotgun", Title = "AutoShotgun", Icon = "ui/weapons/autoshotgun.png", Spawnable = true )]
[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
public class AutoShotgun : ProjectileWeapon<BaseProjectile>
{
	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 );
		anim.SetParam( "aimat_weight", 1.0f );
	}
}

