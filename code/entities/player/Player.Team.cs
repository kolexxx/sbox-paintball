using Sandbox;

namespace PaintBall
{
	public partial class Player
	{
		[Net, Change] public Team Team { get; set; }
		public TimeSince TimeSinceTeamChanged { get; private set; } = 5f;

		public void SetTeam( Team newTeam )
		{
			TimeSinceTeamChanged = 0f;
			TakeDamage( DamageInfo.Generic( float.MaxValue ) );

			Team oldTeam = Team;
			Tags.Remove( $"{oldTeam.GetString()}player" );
			Team = newTeam;
			Tags.Add( $"{newTeam.GetString()}player" );
			Client.SetInt( "team", (int)newTeam );

			Hud.OnTeamChanged( To.Everyone, Client, newTeam );

			Game.Instance.CurrentGameState.OnPlayerChangedTeam( this, oldTeam, newTeam );
		}

		public void OnTeamChanged( Team oldTeam, Team newTeam )
		{
			if ( IsLocalPawn && LifeState == LifeState.Alive )
			{
				Local.Hud.RemoveClass( oldTeam.GetString() );
				Local.Hud.AddClass( newTeam.GetString() );
			}
		}

		[ServerCmd( "changeteam", Help = "Changes the caller's team" )]
		public static void ChangeTeamCommand( Team team )
		{
			Client client = ConsoleSystem.Caller;

			if ( client == null || client.Pawn is not Player player )
				return;

			if ( player.Team == team || player.TimeSinceTeamChanged < 5f )
			{
				if ( team == Team.None )
					Hud.CloseTeamSelect();

				return;
			}

			if ( team == Team.None )
			{
				player.SetTeam( team );
				Hud.CloseTeamSelect();

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
			}
			else
			{
				if ( team == Team.Blue && (blueCount < redCount) )
					player.SetTeam( team );
				else if ( team == Team.Red && (redCount < blueCount) )
					player.SetTeam( team );
			}

			if ( player.Team == team )
				Hud.CloseTeamSelect();
		}
	}
}
