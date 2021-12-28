using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

namespace PaintBall
{
	public class KillConfirmed : Panel
	{
		private KillConfirmedEntry _currentPopUp;

		public KillConfirmed()
		{
			StyleSheet.Load( "/ui/popup/KillConfirmed.scss" );
		}

		[PBEvent.Player.Killed]
		public void OnPlayerKilled( Player player, Entity attacker )
		{
			if ( attacker != Local.Pawn )
				return;

			_currentPopUp?.Delete();
			_currentPopUp = AddChild<KillConfirmedEntry>();
			_currentPopUp.Name.Text = $"You killed {player.Client.Name}";
			_currentPopUp.Icon.SetTexture( $"avatar:{player.Client.PlayerId}" );
		}

		[PBEvent.Round.New]
		private void RoundStart()
		{
			_currentPopUp?.Delete();
		}

		public class KillConfirmedEntry : Panel
		{
			public Image Icon { get; set; }
			public Label Name { get; set; }

			public KillConfirmedEntry()
			{
				Icon = Add.Image( "", "icon" );
				Name = Add.Label( "", "name" );

				_ = DeleteAsync();
			}

			private async Task DeleteAsync()
			{
				await Task.Delay( 5000 );

				Delete();
			}
		}
	}
}
