using Sandbox;

namespace PaintBall
{
	public partial class Player
	{
		public void SetTeam( Team newTeam )
		{
			TakeDamage( DamageInfo.Generic( float.MaxValue ) );

			Team oldTeam = Team;
			Tags.Remove( $"{oldTeam.GetString()}player" );
			Team = newTeam;
			Tags.Add( $"{newTeam.GetString()}player" );
			Client.SetInt( "team", (int)newTeam );

			Hud.OnTeamChanged( To.Everyone, Client, newTeam );

			Game.Instance.CurrentGameState.OnPlayerChangedTeam( this, oldTeam, newTeam );
		}

		[ServerCmd( "changeteam", Help = "Changes the caller's team" )]
		public static void ChangeTeamCommand( Team team )
		{
			Client client = ConsoleSystem.Caller;

			if ( client == null || client.Pawn is not Player player )
				return;

			if ( team != player.Team )
			{
				if ( team == Team.None )
				{
					player.SetTeam( team );

					return;
				}

				int redCount = Team.Red.GetCount();
				int blueCount = Team.Blue.GetCount();

				if ( player.Team == Team.None )
				{
					if ( team == Team.Blue && (blueCount <= redCount) )
						player.SetTeam( team );
					else if ( team == Team.Red && (redCount <= blueCount) )
						player.SetTeam( team );

					return;
				}

				if ( blueCount == redCount )
					return;

				if ( team == Team.Blue && (blueCount < redCount) )
					player.SetTeam( team );
				else if ( team == Team.Red && (redCount < blueCount) )
					player.SetTeam( team );
			}
		}
	}
}
