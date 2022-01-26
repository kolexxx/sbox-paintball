﻿using Paintball.UI;
using Sandbox;
using System;
using System.Linq;

namespace Paintball;

public enum RoundState : byte
{
	None,
	/// <summary>
	/// Players aren't able to move and can buy weapons.
	/// </summary>
	Freeze,
	/// <summary>
	/// Players are able to move freely and do objectives.
	/// </summary>
	Play,
	/// <summary>
	/// The bomb has been planted and it can explode or be defused.
	/// </summary>
	Bomb,
	/// <summary>
	/// Rest period between rounds.
	/// </summary>
	End
}

public partial class GameplayState : BaseState
{
	[ServerVar( "pb_freeze_duration", Help = "The duration of the freeze period." )]
	public static int FreezeDuration { get; set; } = 5;
	[ServerVar( "pb_play_duration", Help = "The duration of the play period." )]
	public static int PlayDuration { get; set; } = 60;
	[ServerVar( "pb_end_duration", Help = "The duration of the end period." )]
	public static int EndDuration { get; set; } = 5;
	[ServerVar( "pb_bomb_duration", Help = "The time needed for the bomb to explode." )]
	public static int BombDuration { get; set; } = 30;
	[ServerVar( "pb_round_limit", Help = "The amount of rounds." )]
	public static int RoundLimit { get; set; } = 12;

	[Net, Change] public int AliveBlue { get; private set; } = 0;
	[Net, Change] public int AliveRed { get; private set; } = 0;
	[Net] public int BlueScore { get; private set; } = 0;
	[Net] public int RedScore { get; private set; } = 0;
	public PlantedBomb Bomb { get; set; }
	public RoundState RoundState { get; set; }
	public override bool UpdateTimer => RoundState != RoundState.End;
	private bool _firstBlood = false;
	private int _toWinScore => 7;
	private int _round = 0;

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		if ( player.Alive() )
		{
			AdjustTeam( player.Team, -1 );
			player.Inventory.DropBomb();
		}
	}

	public override void OnPlayerSpawned( Player player )
	{
		Host.AssertServer();

		AdjustTeam( player.Team, 1 );

		player.Inventory.Add( Rand.Int( 1, 2 ) == 1 ? new SMG() : new Shotgun(), true );
		player.Inventory.Add( new Pistol() );
		player.Inventory.Add( new Knife() );

		if ( Rand.Int( 1, 3 ) == 1 )
			player.Inventory.Add( new Throwable() );

		base.OnPlayerSpawned( player );
	}

	public override void OnPlayerKilled( Player player, Entity attacker, DamageInfo info )
	{
		AdjustTeam( player.Team, -1 );

		if ( !_firstBlood && attacker is Player )
		{
			Audio.AnnounceAll( "first_blood", Audio.Priority.Medium );
			_firstBlood = true;
		}

		player.MakeSpectator();
	}

	public override void OnPlayerChangedTeam( Player player, Team oldTeam, Team newTeam ) { }

	public override void OnSecond()
	{
		if ( Host.IsServer )
		{
			if ( UntilStateEnds )
				RoundStateFinish();
		}
	}

	public override void Tick()
	{
		base.Tick();

		switch ( RoundState )
		{
			case RoundState.Freeze:

				break;

			case RoundState.Play:

				if ( !Host.IsServer )
					break;

				if ( AliveBlue == 0 || AliveRed == 0 )
					RoundStateFinish();

				break;

			case RoundState.Bomb:

				if ( !Host.IsServer )
					break;

				if ( Bomb.IsValid() )
					Bomb.Tick();

				if ( AliveBlue == 0 || Bomb.Disabled )
					RoundStateFinish();

				break;

			case RoundState.End:

				if ( Host.IsServer && Bomb.IsValid() )
					Bomb.Tick();

				break;
		}
	}

	public override void Start()
	{
		base.Start();

		foreach ( var player in Players )
			player.Reset();

		RoundState = RoundState.Freeze;

		if ( Host.IsServer )
			RoundStateStart();
	}

	public void RoundStateStart()
	{
		switch ( RoundState )
		{
			case RoundState.Freeze:

				Bomb?.Delete();
				Bomb = null;

				_firstBlood = false;

				TeamBalance();

				int index = Rand.Int( 1, Team.Red.GetCount() );

				foreach ( var player in Players )
				{
					if ( !player.IsValid() || player.Team == Team.None )
						continue;

					player.Inventory.DeleteContents();

					player.Respawn();
					if ( player.Team == Team.Red && --index == 0 )
						player.Inventory.Add( new Bomb() );
				}

				if ( BlueScore == _toWinScore - 1 || RedScore == _toWinScore - 1 )
					Notification.Create( To.Everyone, "Matchpoint!", FreezeDuration );

				Event.Run( PBEvent.Round.New );
				RPC.OnRoundStateChanged( RoundState.Freeze );

				UntilStateEnds = FreezeDuration;

				break;

			case RoundState.Play:

				Audio.AnnounceAll( "prepare", Audio.Priority.High );

				Event.Run( PBEvent.Round.Start );
				RPC.OnRoundStateChanged( RoundState.Play );

				UntilStateEnds = PlayDuration;

				break;

			case RoundState.Bomb:

				UntilStateEnds = Bomb.TimeUntilExplode;

				break;

			case RoundState.End:

				Event.Run( PBEvent.Round.End, GetWinner() );
				RPC.OnRoundStateChanged( RoundState.End, GetWinner() );

				UntilStateEnds = EndDuration;

				break;
		}

		// Call OnSecond() as soon as RoundState starts
	}

	public void RoundStateFinish()
	{
		switch ( RoundState )
		{
			case RoundState.Freeze:

				RoundState = RoundState.Play;

				break;

			case RoundState.Play:

				RoundState = RoundState.End;

				_ = GetWinner() == Team.Blue ? BlueScore++ : RedScore++;

				break;

			case RoundState.Bomb:

				RoundState = RoundState.End;

				_ = GetWinner() == Team.Blue ? BlueScore++ : RedScore++;

				break;

			case RoundState.End:

				_round++;

				if ( BlueScore == _toWinScore || RedScore == _toWinScore || _round >= RoundLimit )
				{
					Bomb?.Delete();
					Bomb = null;
					Game.Current.ChangeState( new GameFinishedState() );

					return;
				}

				RoundState = RoundState.Freeze;

				AliveBlue = 0;
				AliveRed = 0;

				break;
		}

		RoundStateStart();
	}

	private void AdjustTeam( Team team, int num )
	{
		if ( team == Team.None )
			return;

		if ( team == Team.Blue )
		{
			AliveBlue += num;
			AliveBlue = Math.Max( AliveBlue, 0 );
		}
		else
		{
			AliveRed += num;
			AliveRed = Math.Max( AliveRed, 0 );
		}
	}

	private Team GetWinner()
	{
		if ( Bomb.IsValid() && Bomb.Disabled )
		{
			if ( Bomb.Defuser != null )
				return Team.Blue;
			else
				return Team.Red;
		}

		if ( AliveBlue != 0 && AliveRed != 0 )
			return Team.Blue; // Blue always wins

		if ( AliveBlue > AliveRed )
			return Team.Blue;
		else
			return Team.Red;
	}

	private void TeamBalance()
	{
		var teamBlue = Team.Blue.GetAll();
		var teamRed = Team.Red.GetAll();

		int diff = Math.Abs( teamBlue.Count() - teamRed.Count() ) / 2;

		if ( diff <= 0 )
			return;

		Notification.Create( To.Everyone, "Team have been Auto-Balanced!", 3 );

		Team teamLess = teamBlue.Count() > teamRed.Count() ? Team.Red : Team.Blue;
		var teamMore = teamLess == Team.Blue ? teamRed : teamBlue;

		foreach ( var player in teamMore )
		{
			player.SetTeam( teamLess );

			if ( --diff == 0 )
				break;
		}
	}

	private void OnAliveBlueChanged()
	{
		RoundInfo.Instance.AliveBlue.Text = AliveBlue.ToString();
	}

	private void OnAliveRedChanged()
	{
		RoundInfo.Instance.AliveRed.Text = AliveRed.ToString();
	}
}
