using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paintball;

public partial class Map
{
	public Package Info { get; set; }
	public List<PlayerSpawnPoint> SpawnPoints { get; set; }
	public List<SpectatePoint> SpectatePoints { get; set; }

	public Map() { Event.Register( this ); }

	~Map() { Event.Unregister( this ); }

	public async Task GetInfo()
	{
		Info = await Package.Fetch( Global.MapName, false );

		if ( Info != null )
			Event.Run( PBEvent.Game.Map.InfoFetched );
	}

	[Event.Entity.PostSpawn]
	private void EntityPostSpawn()
	{
		if ( Host.IsServer )
			SpawnPoints = Entity.All.OfType<PlayerSpawnPoint>().ToList();

		SpectatePoints = Entity.All.OfType<SpectatePoint>().ToList();

		_ = GetInfo();
	}

	public void CleanUp()
	{
		if ( Host.IsServer )
		{
			Sandbox.Internal.Decals.RemoveFromWorld();

			foreach ( var entity in Entity.All )
			{
				if ( entity is BaseProjectile || entity is Grenade )
					entity.Delete();
				else if ( entity is Carriable weapon && weapon.IsValid() )
				{
					if ( weapon.Owner == null || weapon is Bomb )
						weapon.Delete();
					else
						weapon.Reset();
				}
			}

			foreach ( var spawnPoint in SpawnPoints )
				spawnPoint.Occupied = false;
		}
		else if ( Host.IsClient )
		{
			foreach ( var ent in Entity.All.OfType<ModelEntity>() )
			{
				if ( ent.IsValid() && ent.IsClientOnly && ent is not BaseViewModel )
					ent.Delete();
			}
		}
	}
}
