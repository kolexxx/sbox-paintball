using Sandbox;

namespace Paintball;

[Item( SlotType.Utility )]
[Buyable( Price = 1000 )]
[Library( "pb_spike", Title = "Spike", Icon = "ui/weapons/grenade.png", Spawnable = true )]
[Hammer.EditorModel( "models/grenade/grenade.vmdl" )]
public sealed partial class Throwable : Weapon
{
	public override int ClipSize => 1;
	public override string FireSound => "";
	public override float PrimaryRate => 15f;
	public override float ReloadTime => 2.0f;
	public override string ViewModelPath => "models/grenade/v_grenade.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = ClipSize;

		SetModel( "models/grenade/grenade.vmdl" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 5 );
		anim.SetParam( "aimat_weight", 1.0f );
	}

	public override bool CanPrimaryAttack()
	{
		if ( Owner.IsFrozen || !Input.Released( InputButton.Attack1 ) )
			return false;

		return true;
	}

	public override void AttackPrimary()
	{
		base.AttackPrimary();

		TimeSincePrimaryAttack = 0;

		var trace = Trace.Ray( Owner.EyePos, (Owner.EyeRot.Forward * 40) ).Run();

		Owner.SwitchToBestWeapon();

		if ( !IsServer )
			return;

		using ( Prediction.Off() )
		{
			var ent = new Grenade();
			ent.Rotation = Owner.EyeRot;
			ent.Position = trace.EndPos;
			ent.Velocity = Owner.EyeRot.Forward * 1000 + Vector3.Up * 100;
			ent.Owner = Owner;
			ent.Origin = this;
			ent.Team = Owner.Team;

			Delete();
		}
	}
}
