using Sandbox;

namespace Paintball;

[Library( "pb_spike", Title = "Spike", Icon = "ui/weapons/grenade.png", Spawnable = true )]
[Hammer.EditorModel( "models/grenade/grenade.vmdl" )]
public sealed partial class Throwable : Carriable
{
	public override int ClipSize => 1;
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

		var trace = Trace.Ray( Owner.EyePosition, (Owner.EyeRotation.Forward * 40) ).Run();

		Owner.SwitchToBestWeapon();

		if ( !IsServer )
			return;

		using ( Prediction.Off() )
		{
			var ent = new Grenade
			{
				Rotation = Owner.EyeRotation,
				Position = trace.EndPos,
				Velocity = Owner.EyeRotation.Forward * 1000 + Vector3.Up * 100,
				Owner = Owner,
				Origin = this,
				Team = Owner.Team
			};

			Delete();
		}
	}
}
