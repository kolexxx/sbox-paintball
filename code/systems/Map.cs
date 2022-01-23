using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paintball;

[Library( "pb_map" )]
public partial class Map : Entity
{
	public Package Info { get; set; }
	public List<PlayerSpawnPoint> SpawnPoints { get; set; } = new();
	public List<SpectatePoint> SpectatePoints { get; set; } = new();
	protected Output RoundStart { get; set; }
	protected Output RoundEnd { get; set; }
	protected Output RoundNew { get; set; }

	public Map() { }

	public async Task GetInfo()
	{
		Info = await Package.Fetch( Global.MapName, false );

		if ( Info != null )
			Event.Run( PBEvent.Game.MapInfoFetched );
	}

	[Event.Entity.PostSpawn]
	private void EntityPostSpawn()
	{
		GetInfo();

		if ( Host.IsServer )
		{
			SpawnPoints = Entity.All.OfType<PlayerSpawnPoint>().ToList();
		}

		SpectatePoints = Entity.All.OfType<SpectatePoint>().ToList();
	}

	[PBEvent.Round.Start]
	private void OnRoundStart()
	{
		RoundStart.Fire( this );
	}

	[PBEvent.Round.End]
	private void OnRoundEnd( Team winner )
	{
		RoundEnd.Fire( this );
	}

	[PBEvent.Round.New]
	private void OnNewRound()
	{
		RoundNew.Fire( this );
	}
}
