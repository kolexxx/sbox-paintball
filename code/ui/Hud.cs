using Sandbox;
using Sandbox.UI;

namespace PaintBall
{
	[Library]
	public partial class Hud : HudEntity<RootPanel>
	{
		[ClientRpc]
		public static void AddKillFeed( string left, string right, string method, Team teamLeft, Team teamRight, long lsteamid, long rsteamid )
		{
			KillFeed.Instance.AddEntry( left, right, method, teamLeft, teamRight, lsteamid, rsteamid );
		}

		[ClientRpc]
		public static void OnTeamChanged( Client client, Team newTeam )
		{
			Scoreboard.Instance.UpdateEntry( client, newTeam );
		}

		[ClientRpc]
		public static void Reset()
		{
			UpdateCrosshairMessage();
			KillFeed.Instance.DeleteChildren();
		}

		[ClientRpc]
		public static void UpdateCrosshairMessage( string text = "" )
		{
			(Local.Hud
				.GetChild( 2 )?
				.GetChild( 0 ) as Label)?
				.SetText( text );
		}

		[ClientRpc]
		public static void UpdateTeamScore( Team team, string text = "0" )
		{
			(GameInfo.Instance.Mid.GetChild( (int)team ).GetChild( 0 ) as Label).Text = text;
		}

		

		public Hud()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<Ammo>();              // 0
			RootPanel.AddChild<ChatBox>();           // 1
			RootPanel.AddChild<Crosshair>();         // 2
			RootPanel.AddChild<GameInfo>();          // 3
			RootPanel.AddChild<InventoryBar>();      // 4
			RootPanel.AddChild<KillFeed>();          // 5
			RootPanel.AddChild<Scoreboard>();        // 6
			RootPanel.AddChild<SpectatorControls>(); // 7
			RootPanel.AddChild<TeamIndicator>();     // 8
		}
	}
}
