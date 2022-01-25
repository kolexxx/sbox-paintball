using Sandbox;
using Sandbox.UI;

namespace Paintball;

public partial class Player : ITeamEntity
{
	[Net, Change] public Team Team { get; set; }
	public TimeSince TimeSinceTeamChanged { get; private set; } = 5f;

	public void SetTeam( Team newTeam )
	{
		TimeSinceTeamChanged = 0f;
		TakeDamage( DamageInfo.Generic( float.MaxValue ) );

		Team oldTeam = Team;
		Tags.Remove( $"{oldTeam.GetString()}" );

		Team = newTeam;
		Tags.Add( $"{newTeam.GetString()}" );

		Client.SetInt( "team", (int)newTeam );

		Event.Run( PBEvent.Player.Team.Changed, this, oldTeam );

		ChatBox.AddInformation( To.Everyone, $"{Client.Name} has joined {newTeam.GetName()}", $"avatar:{Client.PlayerId}" );

		Game.Current.State.OnPlayerChangedTeam( this, oldTeam, newTeam );
	}

	public void OnTeamChanged( Team oldTeam, Team newTeam )
	{
		if ( IsLocalPawn && !IsSpectatingPlayer )
		{
			Local.Hud.RemoveClass( oldTeam.GetString() );
			Local.Hud.AddClass( newTeam.GetString() );
		}

		Event.Run( PBEvent.Player.Team.Changed, this, oldTeam );
	}

	[ServerCmd( "changeteam", Help = "Changes the caller's team" )]
	public static void ChangeTeamCommand( Team team )
	{
		Client client = ConsoleSystem.Caller;

		if ( client == null || client.Pawn is not Player player )
			return;

		if ( player.Team == team || player.TimeSinceTeamChanged <= 5f )
			return;

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
		}
		else
		{
			if ( team == Team.Blue && (blueCount < redCount) )
				player.SetTeam( team );
			else if ( team == Team.Red && (redCount < blueCount) )
				player.SetTeam( team );
		}
	}
}
