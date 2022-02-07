using Sandbox;

namespace Paintball;

[Item( SlotType.Primary )]
[Buyable( Price = 1000 )]
[Library( "pb_shotgun", Title = "Shotgun", Icon = "ui/weapons/shotgun.png", Spawnable = true )]
[Hammer.EditorModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" )]
public partial class Shotgun : ProjectileWeapon<BaseProjectile>
{
	public override int BulletsPerFire => 9;
	public override int ClipSize => 5;
	public override ItemConfig Config { get; set; }
	public override string CrosshairClass => "shotgun";
	public override float MovementSpeedMultiplier => 0.85f;
	public override float PrimaryRate => 1f;
	public override float ProjectileGravity => 7f;
	public override float ProjectileSpeed => 3000f;
	public override float ReloadTime => 0.7f;
	public override float Spread => 0.05f;
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	private bool _attackedDuringReload = false;

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = ClipSize;
		ReserveAmmo = 2 * ClipSize;

		SetModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
	}

	public override void ActiveStart( Entity entity )
	{
		base.ActiveStart( entity );

		_attackedDuringReload = false;
		TimeSinceReload = 0f;
	}

	public override bool CanReload()
	{
		if ( AmmoClip >= ClipSize || (!UnlimitedAmmo && ReserveAmmo == 0) )
			return false;

		if ( !Owner.IsValid() || !Input.Down( InputButton.Reload ) )
			return false;

		var rate = PrimaryRate;
		if ( rate <= 0 )
			return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public override void Simulate( Client owner )
	{
		base.Simulate( owner );

		if ( IsReloading && Input.Pressed( InputButton.Attack1 ) )
			_attackedDuringReload = true;
	}

	public override void OnReloadFinish()
	{
		IsReloading = false;

		TimeSincePrimaryAttack = 0;

		AmmoClip += TakeAmmo( 1 );

		if ( !_attackedDuringReload && AmmoClip < ClipSize && (UnlimitedAmmo || ReserveAmmo != 0) )
			Reload();
		else
			FinishReload();

		_attackedDuringReload = false;
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 3 );
	}

	[ClientRpc]
	public void FinishReload()
	{
		ViewModelEntity?.SetAnimBool( "reload_finished", true );
	}
}
