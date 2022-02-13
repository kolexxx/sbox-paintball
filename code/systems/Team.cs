using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Paintball;

public enum Team : byte
{
	None,
	Blue,
	Red
}

public static class TeamExtensions
{
	public static Color GetColor( this Team team )
	{
		switch ( team )
		{
			case Team.Blue:
				return Color.FromBytes( 0, 146, 255 );
			case Team.Red:
				return Color.Red;
			default:
				return Color.White;
		}
	}

	public static string GetTag( this Team team )
	{
		switch ( team )
		{
			case Team.Blue:
				return "blue";
			case Team.Red:
				return "red";
			default:
				return "none";
		}
	}

	public static string GetName( this Team team )
	{
		switch ( team )
		{
			case Team.Blue:
				return Game.Current.Settings.BlueTeamName;
			case Team.Red:
				return Game.Current.Settings.RedTeamName;
			default:
				return "none";
		}
	}

	public static IEnumerable<Player> GetAll( this Team team )
	{
		return Entity.All.OfType<Player>().Where( e => e.Team == team );
	}

	public static int GetCount( this Team team )
	{
		return Entity.All.OfType<Player>().Where( e => e.Team == team ).Count();
	}

	public static To ToClients( this Team team )
	{
		return To.Multiple( Client.All.Where( x => (x.Pawn as Player).Team == team ) );
	}
}
