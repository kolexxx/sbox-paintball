using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paintball;

public partial class Map : Entity
{
	public Package Info { get; set; }
	public List<PlayerSpawnPoint> SpawnPoints { get; set; }
	public List<SpectatePoint> SpectatePoints { get; set; }
	public MapSettings Settings { get; set; }

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
		if ( Host.IsServer )
		{
			SpawnPoints = Entity.All.OfType<PlayerSpawnPoint>().ToList();
		}

		SpectatePoints = Entity.All.OfType<SpectatePoint>().ToList();
		Settings = Entity.All.FirstOrDefault( x => x is MapSettings ) as MapSettings;

		if ( !Settings.IsValid() )
			Settings = new MapSettings();

		Event.Run( PBEvent.Game.MapSettingsLoaded );

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

[Library( "pb_map_settings" )]
public partial class MapSettings : Entity
{
	[Property] public string BlueTeamName { get; set; } = "Blue";
	[Property] public string RedTeamName { get; set; } = "Red";
	protected Output RoundStart { get; set; }
	protected Output RoundEnd { get; set; }
	protected Output RoundNew { get; set; }

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
