using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace PaintBall
{
	/// <summary>
	/// Header notification. Might change the name later.
	/// </summary>
	public partial class Notification : Popup
	{
		public Label Message { get; set; }
		private static Notification s_current;

		public Notification( string text, float lifeTime ) : base( lifeTime )
		{
			s_current = this;

			StyleSheet.Load( "/ui/Notification.scss" );

			Message = Add.Label( text, "text" );
			BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
		}

		public Notification( string text, Func<bool> condition ) : base( condition )
		{
			s_current = this;

			StyleSheet.Load( "/ui/Notification.scss" );

			Message = Add.Label( text, "text" );
			BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
		}

		[ClientRpc]
		public static void Create( string text = "", float lifeTime = 0f )
		{
			if ( s_current != null && !s_current.HasLifetime )
			{
				s_current.Message.Text = text;
				return;
			}

			s_current?.Delete( true );
			Local.Hud.AddChild( new Notification( text, lifeTime ) );
			s_current = Local.Hud.GetChild( Local.Hud.ChildrenCount - 1 ) as Notification;
		}

		[PBEvent.Round.End]
		private static void RoundEnd( Team winner )
		{
			if ( !Host.IsClient )
				return;

			// We only want to create a standard notification if the bomb didn't trigger the round to end

			Create( $"{winner } wins!", GameplayState.EndDuration );
			Audio.Announce( $"{winner.GetString()}win", Audio.Priority.High );
		}

		// We are creating a notification that will last the entire WaitingForPlayersState
		[PBEvent.Game.StateChanged]
		private static void OnStateChanged( BaseState oldState, BaseState newState )
		{
			if ( !Host.IsClient || newState is not WaitingForPlayersState )
				return;

			s_current?.Delete( true );
			Local.Hud.AddChild( new Notification( "Waiting for players...", () => Game.Current.State is not WaitingForPlayersState ) );
			s_current = Local.Hud.GetChild( Local.Hud.ChildrenCount - 1 ) as Notification;
		}

		public override void OnDeleted()
		{
			base.OnDeleted();

			if ( s_current == this )
				s_current = null;
		}
	}
}
