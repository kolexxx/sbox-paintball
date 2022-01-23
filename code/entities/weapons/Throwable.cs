using Sandbox;

namespace PaintBall;

[Library( "pb_spike", Title = "Spike", Spawnable = true )]
[Hammer.EditorModel( "models/grenade/grenade.vmdl" )]
public sealed partial class Throwable : Weapon
{
	public override SlotType Slot => SlotType.Utility;
	public override int ClipSize => 1;
	public override string FireSound => "";
	public override string Icon => "ui/weapons/grenade.png";
	public override float PrimaryRate => 15f;
	public override float ReloadTime => 2.0f;
	public override string ViewModelPath => "models/grenade/v_grenade.vmdl";
	private bool _isHoldingDownAttack = false;

	public override void ActiveStart( Entity entity )
	{
		base.ActiveStart( entity );

		_isHoldingDownAttack = false;
	}

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
		if ( !Game.Current.State.FreezeTime )
			return false;

		if ( Input.Down( InputButton.Attack1 ) )
		{
			_isHoldingDownAttack = true;

			return false;
		}
		else if ( !Input.Down( InputButton.Attack1 ) && _isHoldingDownAttack )
		{
			return true;
		}

		_isHoldingDownAttack = false;
		return false;
	}

	public override void AttackPrimary()
	{
		base.AttackPrimary();

		TimeSincePrimaryAttack = 0;

		var trace = Trace.Ray( Owner.EyePos, (Owner.EyeRot.Forward * 40) ).Run();

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
			ent.Team = (Owner as Player).Team;
			(Owner as Player).SwitchToBestWeapon();
			Delete();
		}
	}
}
