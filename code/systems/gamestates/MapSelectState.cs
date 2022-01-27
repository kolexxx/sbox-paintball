using Paintball.UI;
using Sandbox;
using System.Collections.Generic;

namespace Paintball;

public partial class MapSelectState : BaseState
{
	public override int StateDuration => 15;
	[Net, Change] public IDictionary<string, string> MapImages { get; set; }
	[Net] public IDictionary<long, string> PlayerIdVote { get; set; }
	[Net] public IDictionary<string, int> VoteCount { get; set; }

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		VoteCount[PlayerIdVote[player.Client.PlayerId]]--;
		PlayerIdVote.Remove( player.Client.PlayerId ); // I don't think this is needed
	}

	public override void OnSecond()
	{
		base.OnSecond();

		if ( Host.IsServer && UntilStateEnds )
			Game.Current.ChangeState( new WaitingForPlayersState() );
	}

	public override void Start()
	{
		base.Start();

		if ( !Host.IsServer )
			return;

		foreach ( var player in Players )
			player.Inventory?.DeleteContents();

		VoteCount = new Dictionary<string, int>();
		PlayerIdVote = new Dictionary<long, string>();
	}

	public override void TimeUp()
	{
		base.TimeUp();

		if ( !Host.IsServer )
			return;

		string mostVotedMap = string.Empty;
		int count = int.MinValue;

		foreach ( KeyValuePair<string, int> kvp in VoteCount )
		{
			if ( kvp.Value > count )
			{
				mostVotedMap = kvp.Key;
				count = kvp.Value;
			}
		}

		Global.ChangeLevel( mostVotedMap );
	}

	[ServerCmd]
	public static void SetVote( string map )
	{
		Assert.NotNull( map );

		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() )
			return;

		var state = Game.Current.State as MapSelectState;
		string oldMapVote = state.PlayerIdVote[player.Client.PlayerId];

		if ( oldMapVote == map )
			return;

		if ( oldMapVote != null )
			state.VoteCount[oldMapVote]--;

		state.PlayerIdVote[player.Client.PlayerId] = map;
		state.VoteCount[map]++;
	}
}
