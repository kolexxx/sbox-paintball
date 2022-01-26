using Sandbox;
using Sandbox.UI;
using System;

namespace Paintball.UI;

[UseTemplate]
public class RoundInfo : Panel
{
	public static RoundInfo Instance;
	public Label Timer { get; set; }
	public Label Message { get; set; }
	public Label AliveBlue { get; set; }
	public Label AliveRed { get; set; }
	public Label BlueScore { get; set; }
	public Label RedScore { get; set; }
	public Panel Bottom { get; set; }

	public RoundInfo()
	{
		Instance = this;

		BindClass( "hidden", () => Scoreboard.Instance.IsVisible || TeamSelect.Instance.IsVisible );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible )
			return;

		var player = Local.Pawn;
		if ( player == null )
			return;

		var game = Game.Current;
		if ( game == null )
			return;

		var state = game.State;
		if ( state == null )
			return;

		if ( state.UpdateTimer )
			Timer.Text = TimeSpan.FromSeconds( state.TimeLeftSeconds ).ToString( @"mm\:ss" );
		else
			Timer.Text = "";
	}

	[PBEvent.Round.Start]
	private void OnRoundStart()
	{
		Bottom.SetClass( "show", false );
	}

	[PBEvent.Round.End]
	private void OnRoundEnd( Team winner )
	{
		var gameplayState = Game.Current.State as GameplayState;

		BlueScore.Text = gameplayState.BlueScore.ToString();
		RedScore.Text = gameplayState.RedScore.ToString();
	}

	[PBEvent.Round.New]
	private void OnNewRound()
	{
		Team team = (Local.Pawn as Player).Team;

		if ( team == Team.None )
			return;

		Bottom.SetClass( "show", true );
		Message.Text = $"Playing as {team.GetName()}";
	}

	[PBEvent.Game.StateChanged]
	private void OnStateChanged( BaseState oldState, BaseState newState )
	{
		if ( oldState is not GameplayState || newState is GameplayState )
			return;

		BlueScore.Text = "0";
		RedScore.Text = "0";
		AliveBlue.Text = "0";
		AliveRed.Text = "0";
	}
}
