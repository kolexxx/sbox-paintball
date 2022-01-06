using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class GeneralNotification : Panel
	{
		public static GeneralNotification Instance;
		public Label Message;

		public GeneralNotification()
		{
			Instance = this;

			Message = Add.Label( "", "text" );
			StyleSheet.Load( "/ui/popup/GeneralNotification.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "hidden", Local.Hud.GetChild( 7 ).IsVisible || Local.Hud.GetChild( 10 ).IsVisible || string.IsNullOrEmpty( Message.Text ) );
		}

		public void UpdateMessage( string text = "" )
		{
			Message.Text = text;
		}

		[PBEvent.Round.New]
		public void OnNewRound()
		{
			UpdateMessage();
		}

		[PBEvent.Round.End]
		public void RoundEnd(Team winner )
		{
			UpdateMessage( $"{winner} wins" );
		}
	}
}
