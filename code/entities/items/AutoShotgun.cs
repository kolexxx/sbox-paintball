using Sandbox;

namespace Paintball;

[Library( "pb_autoshotgun", Title = "AutoShotgun", Icon = "ui/weapons/autoshotgun.png", Spawnable = true )]
[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
public class AutoShotgun : ProjectileWeapon<BaseProjectile>
{
	public override bool Automatic => true;
	public override int BulletsPerFire => 3;
	public override int ClipSize => 5;
	public override float MovementSpeedMultiplier => 0.85f;
	public override float PrimaryRate => 3f;
	public override float ProjectileGravity => 7f;
	public override float ProjectileSpeed => 2500f;
	public override float ReloadTime => 3f;

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 );
		anim.SetParam( "aimat_weight", 1.0f );
	}
}

