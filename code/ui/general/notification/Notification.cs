using Sandbox;
using Sandbox.UI;
using System;

namespace Paintball.UI;

/// <summary>
/// Header notification. Might change the name later.
/// </summary>
[UseTemplate]
public partial class Notification : Popup
{
	public Label Message { get; set; }
	private static Notification s_current;

	public Notification( string text, float lifeTime ) : base( lifeTime )
	{
		s_current = this;

		Message.Text = text;
		BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
	}

	public Notification( string text, Func<bool> condition ) : base( condition )
	{
		s_current = this;

		StyleSheet.Load( "/ui/general/notification/Notification.scss" );

		Message.Text = text;
		BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
	}

	[ClientRpc]
	public static void Create( string message = "", float lifeTime = 0f )
	{
		if ( s_current != null && !s_current.HasLifetime )
		{
			s_current.Message.Text = message;
			return;
		}

		s_current?.Delete( true );
		Local.Hud.AddChild( new Notification( message, lifeTime ) );
	}

	[PBEvent.Round.End]
	private static void OnRoundEnd( Team winner )
	{
		if ( !Host.IsClient )
			return;

		var bomb = (Game.Current.State as GameplayState).Bomb;
		string teamName = winner.GetName();

		string message = $"{teamName} win{(teamName[teamName.Length - 1] == 's' ? '\0' : 's')}!";

		if ( winner == Team.Blue && bomb.IsValid() && bomb.Disabled && bomb.Defuser != null )
			message = "Bomb has been defused!";

		Local.Hud.AddChild( new Notification( message, 5 ) );
		Audio.Announce( $"{winner.GetTag()}win", Audio.Priority.High );
	}

	// We are creating a notification that will last the entire WaitingForPlayersState
	[PBEvent.Game.StateChanged]
	private static void OnStateChanged( BaseState _, BaseState newState )
	{
		if ( !Host.IsClient )
			return;

		s_current?.Delete( true );

		if ( newState is WaitingForPlayersState )
			Local.Hud.AddChild( new Notification( "Waiting for players...", () => Game.Current.State is not WaitingForPlayersState ) );
	}

	public override void OnDeleted()
	{
		base.OnDeleted();

		if ( s_current == this )
			s_current = null;
	}
}
