using Sandbox;
using Sandbox.UI;

namespace Paintball.UI;

[UseTemplate]
public class SpectatedPlayerInfo : Panel
{
	public Player TargetPlayer { get; set; }
	public Panel PlayerInfo { get; set; }
	public Image Avatar { get; set; }
	public Label Name { get; set; }
	public Panel SpectatorControls { get; set; }
	private static SpectatedPlayerInfo s_current;

	public SpectatedPlayerInfo( Player player )
	{
		TargetPlayer = player;

		SetClass( TargetPlayer.Team.GetString(), true );

		Avatar.SetTexture( $"avatar:{TargetPlayer.Client.PlayerId}" );
		Name.Text = TargetPlayer.Client.Name;

		(SpectatorControls.GetChild( 0 ) as InputHint).SetButton( InputButton.Attack2 );
		(SpectatorControls.GetChild( 0 ) as InputHint).Context.Text = "Switch to previous player";
		(SpectatorControls.GetChild( 1 ) as InputHint).SetButton( InputButton.Jump );
		(SpectatorControls.GetChild( 1 ) as InputHint).Context.Text = "Change spectate camera";
		(SpectatorControls.GetChild( 2 ) as InputHint).SetButton( InputButton.Attack1 );
		(SpectatorControls.GetChild( 2 ) as InputHint).Context.Text = "Switch to next player";
	}

	[PBEvent.Player.Spectating.Changed]
	private static void OnSpectatedPlayerChanged( Player oldPlayer, Player newPlayer )
	{
		if ( oldPlayer != null )
			s_current.Delete();

		if ( newPlayer == null )
			return;

		Local.Hud.AddChild( new SpectatedPlayerInfo( newPlayer ) );
		s_current = Local.Hud.GetChild( Local.Hud.ChildrenCount - 1 ) as SpectatedPlayerInfo;
	}
}
