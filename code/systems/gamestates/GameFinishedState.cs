using Paintball.UI;
using Sandbox;

namespace Paintball;

public partial class GameFinishedState : BaseState
{
	public override int StateDuration => 5;

	public override void OnSecond()
	{
		base.OnSecond();

		if ( Host.IsServer && UntilStateEnds )
			Game.Current.ChangeState( new WaitingForPlayersState() );
	}

	public override void Start()
	{
		base.Start();

		foreach ( var player in Players )
			player.Inventory?.DeleteContents();

		if ( Host.IsClient )
			Scoreboard.Instance.Show = true;
	}

	public override void Finish()
	{
		base.Finish();

		if ( Host.IsClient )
			Scoreboard.Instance.Show = false;
	}
}
