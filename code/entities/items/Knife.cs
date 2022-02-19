using Sandbox;

namespace Paintball;

[Hammer.Skip]
[Library( "pb_knife", Title = "Knife", Spawnable = false )]
public partial class Knife : Carriable
{
	[Net, Predicted] public TimeSince TimeSinceStab { get; set; }
	[Net, Predicted] public TimeSince TimeSinceSwing { get; set; }
	public override bool Droppable => false;

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < Info.DeployTime )
			return;

		if ( CanAttack() )
			using ( LagCompensation() )
				Attack();
	}

	public void Attack()
	{
		if ( Input.Down( InputButton.Attack1 ) )
			MeleeAttack( 50f, 100f, 8f );
		else if ( Input.Down( InputButton.Attack2 ) )
			MeleeAttack( 100f, 50f, 16f );
	}

	public bool CanAttack()
	{
		if ( Owner.IsFrozen )
			return false;

		if ( Input.Down( InputButton.Attack1 ) && TimeSinceSwing < 0.6f )
			return false;

		if ( Input.Down( InputButton.Attack2 ) && TimeSinceStab < 1f )
			return false;

		return true;
	}


	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 5 );
		anim.SetParam( "aimat_weight", 1.0f );
	}

	protected void MeleeAttack( float damage, float range, float radius )
	{
		TimeSinceStab = 0;
		TimeSinceSwing = 0;

		Owner.SetAnimBool( "b_attack", true );
		SwingEffects();

		var endPos = Owner.EyePosition + Owner.EyeRotation.Forward * range;

		var trace = Trace.Ray( Owner.EyePosition, endPos )
			.UseHitboxes( true )
			.WithoutTags( Owner.Team.GetTag() )
			.Ignore( this )
			.Radius( radius )
			.Run();

		if ( !trace.Hit )
			return;

		trace.Surface.DoBulletImpact( trace );

		if ( !IsServer )
			return;

		using ( Prediction.Off() )
		{
			DamageInfo info = new DamageInfo()
				.UsingTraceResult( trace )
				.WithAttacker( Owner )
				.WithWeapon( this );

			info.Damage = damage;

			trace.Entity.TakeDamage( info );
		}
	}

	[ClientRpc]
	protected void SwingEffects()
	{
		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}
}
