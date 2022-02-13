using Sandbox;

namespace Paintball;

[Library( "pb_havoc", Title = "Havoc", Icon = "ui/weapons/smg.png", Spawnable = true )]
[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
public partial class Havoc : ProjectileWeapon<BouncyProjectile>
{
	public override bool Automatic => true;
	public override int ClipSize => 15;
	public override float MovementSpeedMultiplier => 0.85f;
	public override float PrimaryRate => 6f;
	public override float ProjectileGravity => 4f;
	public override float ProjectileSpeed => 2500f;
	public override float ReloadTime => 3f;

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 );
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
