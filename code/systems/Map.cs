using Sandbox;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Paintball;

public static partial class Map
{
	public static Package Info { get; private set; }
	public static List<Bombsite> Bombsites { get; private set; }
	public static List<PlayerSpawnPoint> BlueSpawnPoints { get; private set; }
	public static List<PlayerSpawnPoint> RedSpawnPoints { get; private set; }
	public static List<SpectatePoint> SpectatePoints { get; private set; }

	public static async Task GetInfo()
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

			Global.Lobby.MaxMembers = Math.Min( BlueSpawnPoints.Count, RedSpawnPoints.Count );
		}

		SpectatePoints = Entity.All.OfType<SpectatePoint>().ToList();
		Bombsites = Entity.All.OfType<Bombsite>().ToList();

		_ = GetInfo();
	}

	public static void CleanUp()
	{
		if ( Host.IsServer )
		{
			Sandbox.Internal.Decals.RemoveFromWorld();
			EntityManager.CleanUpMap( Filter );
		}
		else
		{
			foreach ( var entity in Entity.All )
			{
				if ( entity.IsClientOnly && entity is not BaseViewModel )
					entity.Delete();
			}
		}

		return;
	}

	public static bool Filter( string className, Entity ent )
	{
		if ( className == "player" || className == "worldent" || className == "worldspawn" || className == "soundent" || className == "player_manager" )
			return false;

		// When creating entities we only have classNames to work with..
		if ( ent == null || !ent.IsValid )
			return true;

		// Gamemode related stuff, game entity, HUD, etc
		if ( ent is GameBase || ent.Parent is GameBase )
			return false;

		// Player related stuff, clothing and weapons
		foreach ( var cl in Client.All )
		{
			if ( ent == cl.Pawn || cl.Pawn.Inventory.Contains( ent ) || ent.Parent == cl.Pawn )
				return false;
		}

		return true;
	}
}
