using Sandbox;

namespace Paintball;

public partial class Player : ILook
{
	public Entity Looking { get; set; }

	protected void TickPlayerLook()
	{
		if ( !IsClient || ( !this.Alive() && !IsSpectatingPlayer) )
			return;

		var lastLookingEntity = Looking;

		Looking = IsValidLookEntity( Using ) ? Using : FindLookable();


		if ( lastLookingEntity != Looking )
		{
			(lastLookingEntity as ILook)?.EndLook( CurrentPlayer ); ;
			(Looking as ILook)?.StartLook( CurrentPlayer );

			return;
		}

		if ( Looking == null )
			return;

		if ( !(Looking as ILook).IsLookable( CurrentPlayer ) )	
			StopLooking();
	}

	protected Entity FindLookable()
	{
		var tr = Trace.Ray( CurrentPlayer.EyePos, CurrentPlayer.EyePos + CurrentPlayer.EyeRot.Forward * (105 * Scale) )
			.HitLayer( CollisionLayer.All )
			.Ignore( CurrentPlayer )
			.Run();

		var ent = tr.Entity;

		if ( !IsValidLookEntity( ent ) ) return null;

		return ent;
	}

	protected void StopLooking()
	{
		(Looking as ILook)?.EndLook( CurrentPlayer );
		Looking = null;
	}

	protected bool IsValidLookEntity( Entity entity )
	{
		return entity.IsValid() && entity is ILook iLook && iLook.IsLookable( CurrentPlayer );
	}

	bool ILook.IsLookable( Entity viewer )
	{
		return viewer is Player player && !player.IsSpectatingPlayer && player.Team == Team;
	}

	void ILook.StartLook( Entity viewer )
	{
		// implement NamePlates
	}

	void ILook.EndLook( Entity viewer )
	{
		// delete NamePlate
	}
}
