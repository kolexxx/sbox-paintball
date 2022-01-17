using Sandbox;

namespace PaintBall;

public partial class Player
{
	[Net]
	public new Entity Using { get; set; }
	public Entity Looking { get; set; }

	public bool IsUseDisabled()
	{
		return ActiveChild is IUse use && use.IsUsable( this );
	}

	protected virtual void TickPlayerLook()
	{
		var lastLookingEntity = Looking;

		Looking = FindEntity();

		if ( IsClient )
		{
			if ( lastLookingEntity != Looking )
			{
				(lastLookingEntity as ILook)?.EndLook();
				(Looking as ILook)?.StartLook();
			}
			else
			{
				(lastLookingEntity as ILook)?.Update();
			}
		}

		if ( !IsServer ) return;

		// Turn prediction off
		using ( Prediction.Off() )
		{
			if ( Input.Pressed( InputButton.Use ) )
			{
				Using = FindUsable();

				if ( Using == null )
				{
					UseFail();
					return;
				}
			}

			if ( !Input.Down( InputButton.Use ) )
			{
				StopUsing();
				return;
			}

			if ( !Using.IsValid() )
				return;

			// If we move too far away or something we should probably ClearUse()?

			//
			// If use returns true then we can keep using it
			//
			if ( Using is IUse use && use.OnUse( this ) )
				return;

			StopUsing();
		}
	}

	protected virtual Entity FindEntity()
	{
		var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * (105 * Scale) )
			.HitLayer( CollisionLayer.All )
			.Ignore( this )
			.Run();

		// See if any of the parent entities are usable if we ain't.
		var ent = tr.Entity;

		// Still no good? Bail.
		if ( !ent.IsValid() ) return null;

		return ent;
	}

	protected override Entity FindUsable()
	{
		if ( IsUseDisabled() )
			return null;

		// See if any of the parent entities are usable if we ain't.
		var ent = Looking;
		while ( ent.IsValid() && !IsValidUseEntity( ent ) )
		{
			ent = ent.Parent;
		}

		// Nothing found, try a wider search
		if ( !IsValidUseEntity( ent ) )
		{
			var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * (105 * Scale) )
			.Radius( 2 )
			.HitLayer( CollisionLayer.All )
			.Ignore( this )
			.Run();

			// See if any of the parent entities are usable if we ain't.
			ent = tr.Entity;
			while ( ent.IsValid() && !IsValidUseEntity( ent ) )
			{
				ent = ent.Parent;
			}
		}

		// Still no good? Bail.
		if ( !IsValidUseEntity( ent ) ) return null;

		return ent;
	}

	protected override void UseFail()
	{
		if ( IsUseDisabled() )
			return;

		base.UseFail();
	}
}
