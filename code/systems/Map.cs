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
	public MapSettings Settings { get; set; }

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
		{
			SpawnPoints = Entity.All.OfType<PlayerSpawnPoint>().ToList();
		}

		SpectatePoints = Entity.All.OfType<SpectatePoint>().ToList();

		if ( Settings == null )
			Settings = new MapSettings();

		_ = GetInfo();
	}

	[PBEvent.Round.New]
	public void CleanUp()
	{
		if ( Host.IsServer )
		{
			Sandbox.Internal.Decals.RemoveFromWorld();

			foreach ( var entity in Entity.All )
			{
				if ( entity is BaseProjectile || entity is Grenade )
					entity.Delete();
				else if ( entity is Weapon weapon && weapon.IsValid() && weapon.Owner == null )
					weapon.Delete();
			}

			foreach ( var spawnPoint in SpawnPoints )
				spawnPoint.Occupied = false;
		}

		if ( Host.IsClient )
		{
			foreach ( var ent in Entity.All.OfType<ModelEntity>() )
			{
				if ( ent.IsValid() && ent.IsClientOnly && ent is not BaseViewModel )
					ent.Delete();
			}
		}
	}
}
