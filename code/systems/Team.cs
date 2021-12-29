using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace PaintBall
{
	public enum Team
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

		public static string GetString( this Team team )
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

		public static IEnumerable<Player> GetAll( this Team team )
		{
			return Entity.All.OfType<Player>().Where( e => e.Team == team );
		}

		public static int GetCount( this Team team )
		{
			return Entity.All.OfType<Player>().Where( e => e.Team == team ).Count();
		}
	}
}
