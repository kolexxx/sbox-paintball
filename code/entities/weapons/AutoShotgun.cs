using Sandbox;

namespace Paintball;

[Config( SlotType.Primary )]
[Buyable( Price = 2000 )]
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

