using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintBall;

[Library( "pb_map" )]
public partial class Map : Entity
{
	public Package Info { get; set; }
	public List<PlayerSpawnPoint> SpawnPoints { get; set; } = new();
	public List<SpectatePoint> SpectatePoints { get; set; } = new();
	protected Output RoundStart { get; set; }
	protected Output RounEnd { get; set; }
	protected Output RoundNew { get; set; }

	public Map()
	{
	}

	public async Task GetInfo()
	{
		Info = await Package.Fetch( Global.MapName, false );
		Event.Run( PBEvent.Game.MapInfoFetched );
	}

	[Event.Entity.PostSpawn]
	private void EntityPostSpawn()
	{
		if ( Host.IsServer )
		{
			SpawnPoints = Entity.All.OfType<PlayerSpawnPoint>().ToList();
		}

		SpectatePoints = Entity.All.OfType<SpectatePoint>().ToList();
	}
}
