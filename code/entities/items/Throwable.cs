using Sandbox;

namespace Paintball;

[Library( "pb_spike", Title = "Spike", Icon = "ui/weapons/grenade.png", Spawnable = true )]
[Hammer.EditorModel( "models/grenade/grenade.vmdl" )]
public sealed partial class Throwable : Carriable
{
	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( CanThrow() )
			Throw();
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 5 );
		anim.SetParam( "aimat_weight", 1.0f );
	}

	public bool CanThrow()
	{
		if ( Owner.IsFrozen || !Input.Released( InputButton.Attack1 ) )
			return false;

		return true;
	}

	public void Throw()
	{
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
