using Sandbox;

namespace Paintball;

[Item( SlotType.Primary )]
[Buyable( Price = 1000 )]
[Library( "pb_smg", Title = "SMG", Icon = "ui/weapons/smg.png", Spawnable = true )]
[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
public partial class SMG : ProjectileWeapon<BaseProjectile>
{
	public override bool Automatic => true;
	public override int ClipSize => 30;
	public override ItemConfig Config { get; set; }
	public override float MovementSpeedMultiplier => 0.85f;
	public override float PrimaryRate => 8f;
	public override float ProjectileGravity => 7f;
	public override float ProjectileSpeed => 2500f;
	public override float ReloadTime => 3f;
	public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = ClipSize;
		ReserveAmmo = 2 * ClipSize;

		SetModel( "weapons/rust_smg/rust_smg.vmdl" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 );
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
