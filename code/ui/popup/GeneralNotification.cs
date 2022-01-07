using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

namespace PaintBall
{
	public partial class GeneralNotification : Panel
	{
		public static GeneralNotification Instance;
		private Panel _current;
		public GeneralNotification()
		{
			Instance = this;

			StyleSheet.Load( "/ui/popup/GeneralNotification.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			SetClass( "hidden", Local.Hud.GetChild( 7 ).IsVisible || Local.Hud.GetChild( 10 ).IsVisible );
		}

		public void CreateNotification( string text = "", int time = 0 )
		{
			_current?.Delete();

			_current = Add.Panel( "notification" );
			_current.Add.Label( text, "text" );
		}

		[PBEvent.Round.New]
		public void OnNewRound()
		{
			Instance.DeleteChildren();
		}

		[PBEvent.Round.Start]
		public void RoundStart()
		{
			Instance.DeleteChildren();
		}

		[PBEvent.Round.End]
		public void RoundEnd( Team winner )
		{
			CreateNotification( $"{winner} wins", MainGameState.EndDuration );
		}

		[ClientRpc]
		public static void Create( string text = "", int time = 0 )
		{
			Instance.CreateNotification( text, time );
		}

		private async Task DeleteAsync( int time )
		{
			await Task.Delay( time * 1000 );

			_current.Delete();
		}
	}
}
