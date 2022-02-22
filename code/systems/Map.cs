using Sandbox;
using Sandbox.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paintball;

public static partial class Map
{
	public static MapAccessor Accessor => GlobalGameNamespace.Map;
	public static Package Info { get; private set; }
	public static List<Bombsite> Bombsites { get; private set; }
	public static List<PlayerSpawnPoint> BlueSpawnPoints { get; private set; }
	public static List<PlayerSpawnPoint> RedSpawnPoints { get; private set; }
	public static List<PointCamera> SpectatePoints { get; private set; }

	private static async Task GetInfo()
	{
		Info = await Package.Fetch( Global.MapName, false );

		if ( Info != null )
			Event.Run( PBEvent.Game.Map.InfoFetched );
	}

	[Event.Entity.PostSpawn]
	private static void EntityPostSpawn()
	{
		if ( Host.IsServer )
		{
			BlueSpawnPoints = new();
			RedSpawnPoints = new();

			foreach ( var spawnpoint in Entity.All.OfType<PlayerSpawnPoint>() )
			{
				if ( spawnpoint.Team == Team.Blue )
					BlueSpawnPoints.Add( spawnpoint );
				else if ( spawnpoint.Team == Team.Red )
					RedSpawnPoints.Add( spawnpoint );
			}
		}

		SpectatePoints = Entity.All.OfType<PointCamera>().ToList();
		Bombsites = Entity.All.OfType<Bombsite>().ToList();

		_ = GetInfo();
	}

	public static void CleanUp()
	{
		Sandbox.Internal.Decals.RemoveFromWorld();
		Accessor.Reset( Game.DefaultCleanupFilter );
	}
}
