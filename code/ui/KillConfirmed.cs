using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace PaintBall
{
	public class KillConfirmed : Panel
	{
		public KillEntry Current;

		public KillConfirmed() { }

		[PBEvent.Player.Killed]
		public void OnPlayerKilled( Player player )
		{
			if ( !player.IsValid() || player.LastAttacker != Local.Pawn )
				return;

			Current?.Delete();
			Current = AddChild<KillEntry>();
			Current.Name.Text = player.Client.Name;
			Current.Icon.SetTexture( $"avatar:{player.Client.PlayerId}" );
		}

		[PBEvent.Round.End]
		private void RoundEnd()
		{
			Current?.Delete();
		}

		[UseTemplate]
		public class KillEntry : Panel
		{
			public Image Icon { get; set; }
			public Label Name { get; set; }

			public KillEntry() { _ = DeleteAsync(); }

			private async Task DeleteAsync()
			{
				await Task.Delay( 5000 );

				Delete();
			}
		}
	}
}
