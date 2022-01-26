using Sandbox;
using Sandbox.UI;

namespace Paintball.UI;

[UseTemplate]
public partial class TeamSelect : Panel
{
	public static TeamSelect Instance;
	public Button Blue { get; set; }
	public Button Red { get; set; }
	public Label Timer { get; set; }
	public Label ServerInfo { get; set; }
	public Image MapImage { get; set; }
	private TimeSince _timeSinceOpened = 0f;
	private bool _open = true;

	public TeamSelect()
	{
		Instance = this;
		ServerInfo.Text = Global.Lobby.Title + " | " + Global.MapName;
	}

	public override void Tick()
	{
		base.Tick();

		if ( Input.Pressed( InputButton.Menu ) && _timeSinceOpened > 0.1f )
		{
			_open = !_open;
			_timeSinceOpened = 0f;
		}

		SetClass( "open", _open );

		if ( !IsVisible )
			return;

		Timer.Text = RoundInfo.Instance.Timer.Text;
	}

	public void BecomeSpectator()
	{
		Player.ChangeTeamCommand( Team.None );
	}

	public void JoinBlue()
	{
		Player.ChangeTeamCommand( Team.Blue );
	}

	public void JoinRed()
	{
		Player.ChangeTeamCommand( Team.Red );
	}

	[PBEvent.Player.Team.Changed]
	public void OnPlayerTeamChanged( Player player, Team oldTeam )
	{
		if ( player.IsLocalPawn )
			Close();
	}

	private void Close()
	{
		_open = false;
	}

	[PBEvent.Game.Map.SettingsLoaded]
	private void OnMapSettingsLoaded()
	{
		Blue.Text = "Join " + Game.Current.Map.Settings.BlueTeamName;
		Red.Text = "Join " + Game.Current.Map.Settings.RedTeamName;
	}

	[PBEvent.Game.Map.InfoFetched]
	private void OnMapInfoFetched()
	{
		MapImage.SetTexture( Game.Current.Map.Info.Thumb );
		ServerInfo.Text = Global.Lobby.Title + " | " + Game.Current.Map.Info.Title;
	}
}
