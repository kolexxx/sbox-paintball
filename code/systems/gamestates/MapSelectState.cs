﻿using Paintball.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

		if ( PlayerIdVote[player.Client.PlayerId] != string.Empty )
			VoteCount[PlayerIdVote[player.Client.PlayerId]]--;

		PlayerIdVote.Remove( player.Client.PlayerId ); // I don't think this is needed
	}

	public override void Start()
	{
		base.Start();

		if ( !Host.IsServer )
		{
			Local.Hud.AddChild<MapSelect>();

			return;
		}

		MapImages = new Dictionary<string, string>();
		PlayerIdVote = new Dictionary<long, string>();
		VoteCount = new Dictionary<string, int>();

		foreach ( var player in Players )
		{
			player.Inventory?.DeleteContents();
			PlayerIdVote.Add( player.Client.PlayerId, string.Empty );
		}

		_ = Load();
	}

	public override void TimeUp()
	{
		base.TimeUp();

		if ( !Host.IsServer )
			return;

		if ( MapImages.Count == 0 )
		{
			Global.ChangeLevel( "kole.pb_snow" );

			return;
		}

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

		if ( oldMapVote != string.Empty )
			state.VoteCount[oldMapVote]--;
		else
			state.UntilStateEnds = Math.Min( state.UntilStateEnds + 1, state.StateDuration );

		state.PlayerIdVote[player.Client.PlayerId] = map;
		state.VoteCount[map]++;
	}

	public async Task Load()
	{
		List<string> mapNames = await GetMapNames();
		List<string> mapImages = await GetMapImages( mapNames );

		for ( int i = 0; i < mapNames.Count; ++i )
		{
			MapImages[mapNames[i]] = mapImages[i];
			VoteCount[mapNames[i]] = 0;
		}
	}

	private async Task<List<string>> GetMapNames()
	{
		Package result = await Package.Fetch( Global.GameName, true );
		return result.GameConfiguration.MapList;
	}

	private async Task<List<string>> GetMapImages( List<string> mapNames )
	{
		List<string> mapThumbnails = new();
		for ( int i = 0; i < mapNames.Count; ++i )
		{
			Package result = await Package.Fetch( mapNames[i], true );
			mapThumbnails.Add( result.Thumb );
		}

		return mapThumbnails;
	}

	private void OnMapImagesChanged()
	{
		MapSelect.Instance?.LoadMaps();
	}
}
