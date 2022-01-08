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
		public static void UpdateTeamScore( Team team, string text = "0" )
		{
			(RoundInfo.Instance.Middle.GetChild( (int)team ).GetChild( 0 ) as Label).Text = text;
		}

		public Hud()
		{
			if ( !IsClient )
				return;

			RootPanel.StyleSheet.Load( "/ui/Hud.scss" );

			RootPanel.AddChild<Ammo>();                  // 0
			RootPanel.AddChild<ChatBox>();               // 1
			RootPanel.AddChild<InventoryBar>();          // 3
			RootPanel.AddChild<KillFeed>();              // 6
			RootPanel.AddChild<RoundInfo>();             // 7
			RootPanel.AddChild<Scoreboard>();            // 8
			RootPanel.AddChild<TeamIndicator>();         // 9
			RootPanel.AddChild<TeamSelect>();            // 10
			RootPanel.AddChild<VoiceList>();             // 11
		}
	}
}
