using Sandbox;

namespace Paintball;

[Library( "pb_smg", Title = "SMG", Icon = "ui/weapons/smg.png", Spawnable = true )]
[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
public partial class SMG : ProjectileWeapon<BaseProjectile>
{
	public override bool Automatic => true;
	public override int ClipSize => 30;
	public override float MovementSpeedMultiplier => 0.85f;
	public override float PrimaryRate => 8f;
	public override float ProjectileGravity => 7f;
	public override float ProjectileSpeed => 2500f;
	public override float ReloadTime => 3f;

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 );
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
