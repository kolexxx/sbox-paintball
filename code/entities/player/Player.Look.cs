using Sandbox;

namespace PaintBall;

public partial class Player
{
	public Entity Looking { get; set; }

	protected void TickPlayerLook()
	{
		if ( !IsClient )
			return;

		var lastLookingEntity = Looking;

		Looking = IsValidLookEntity( Using ) ? Using : FindLookable();

		if ( lastLookingEntity != Looking )
		{
			(lastLookingEntity as ILook)?.EndLook();
			(Looking as ILook)?.StartLook();

			return;
		}

		if ( Looking == null )
			return;

		if ( (Looking as ILook).IsLookable( this ) )
		{
			(Looking as ILook).Update();
		}
		else
		{
			(Looking as ILook).EndLook();
			Looking = null;
		}
	}

	protected Entity FindLookable()
	{
		var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * (105 * Scale) )
			.HitLayer( CollisionLayer.All )
			.Ignore( this )
			.Run();

		var ent = tr.Entity;

		if ( !IsValidLookEntity( ent ) ) return null;

		return ent;
	}

	protected bool IsValidLookEntity( Entity entity )
	{
		return entity.IsValid() && entity is ILook iLook && iLook.IsLookable( this );
	}
}
