using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace PaintBall;

public partial class Map : Entity
{
	public new string Name => Global.MapName;
	public List<PlayerSpawnPoint> SpawnPoints { get; set; } = new();
	public List<SpectatePoint> SpectatePoints { get; set; } = new();	
	protected Output RoundStart { get; set;}
	protected Output RounEnd { get; set; }
	protected Output RoundNew { get; set; }

	public Map()
	{

	}

	[Event.Entity.PostSpawn]
	private void EntityPostSpawn()
	{
		Debug.CheckRealms();

		if ( Host.IsServer )
		{
			SpawnPoints = Entity.All.OfType<PlayerSpawnPoint>().ToList();
		}

		SpectatePoints = Entity.All.OfType<SpectatePoint>().ToList();
	}
}
