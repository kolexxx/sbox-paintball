using Paintball.UI;
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
	[Net] public TimeUntil BuyTimeExpire { get; private set; } = 0;
	public PlantedBomb Bomb { get; set; }
	public bool BombEnabled { get; init; } = true;
	public override bool CanBuy => !BuyTimeExpire;
	public int Round { get; private set; } = 1;
	public RoundState RoundState { get; set; }
	public int ToWinScore { get; private set; }
	public override bool UpdateTimer => RoundState != RoundState.End;
	private bool _firstBlood = false;

	public GameplayState() : base() { }

	public GameplayState( bool bombEnabled ) : base() => BombEnabled = bombEnabled;

	public override void OnPlayerJoin( Player player )
	{
		base.OnPlayerJoin( player );

		if ( RoundState == RoundState.Play || RoundState == RoundState.Bomb )
			if ( Players.Count == 2 )
				RoundStateFinish();
	}

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		player.Inventory.DropBomb();

		CheckRoundOver();
	}

	public override void OnPlayerSpawned( Player player )
	{
		base.OnPlayerSpawned( player );

		if ( player.Inventory.HasFreeSlot( SlotType.Secondary ) )
			player.Inventory.Add( new Pistol() );
		if ( player.Inventory.HasFreeSlot( SlotType.Melee ) )
			player.Inventory.Add( new Knife() );

		if ( RoundState == RoundState.Play || RoundState == RoundState.Bomb )
			if ( AliveBlue == 0 || AliveRed == 0 )
				RoundStateFinish();
	}

	public override void OnPlayerKilled( Player player, Entity attacker, DamageInfo info )
	{
		base.OnPlayerKilled( player, attacker, info );

		if ( !_firstBlood && attacker is Player )
		{
			Audio.AnnounceAll( "first_blood", Audio.Priority.Medium );
			_firstBlood = true;
		}

		player.MakeSpectator();
		CheckRoundOver();
	}

	public override void OnPlayerChangedTeam( Player player, Team oldTeam, Team newTeam ) { }

	public override void Tick()
	{
		if ( Bomb.IsValid() )
			Bomb.Tick();

		if ( UntilStateEnds )
			TimeUp();
	}

	public override void TimeUp()
	{
		base.TimeUp();

		if ( Host.IsServer )
			RoundStateFinish();
	}

	public override void Start()
	{
		base.Start();

		foreach ( var player in Players )
			player.Reset();

		RoundState = RoundState.Freeze;
		ToWinScore = (Game.Current.Settings.RoundLimit >> 1) + 1;

		if ( Host.IsServer )
			RoundStateStart();
	}

	public void RoundStateStart()
	{
		switch ( RoundState )
		{
			case RoundState.Freeze:
			{
				if ( Host.IsClient )
				{
					if ( BlueScore == ToWinScore - 1 || RedScore == ToWinScore - 1 )
						Notification.Create( "Matchpoint!", Game.Current.Settings.FreezeDuration );

					return;
				}

				Event.Run( PBEvent.Round.New );
				RPC.OnRoundStateChanged( RoundState.Freeze );

				Bomb = null;
				_firstBlood = false;

				TeamBalance();

				int index = BombEnabled ? Rand.Int( 1, Team.Red.GetCount() ) : int.MaxValue;

				foreach ( var player in Players )
				{
					if ( !player.IsValid() || player.Team == Team.None )
						continue;

					player.Respawn();

					if ( player.Team == Team.Red && --index == 0 )
						player.Inventory.Add( new Bomb() );
				}

				UntilStateEnds = Game.Current.Settings.FreezeDuration;
				BuyTimeExpire = Game.Current.Settings.FreezeDuration + 10;

				break;
			}
			case RoundState.Play:
			{
				if ( Host.IsClient )
				{
					Audio.Announce( "prepare", Audio.Priority.High );

					return;
				}

				Event.Run( PBEvent.Round.Start );
				RPC.OnRoundStateChanged( RoundState.Play );

				UntilStateEnds = Game.Current.Settings.PlayDuration;

				break;
			}
			case RoundState.Bomb:
			{
				// clientside is handled somewhere else

				UntilStateEnds = Bomb.TimeUntilExplode;

				break;
			}
			case RoundState.End:
			{
				if ( Host.IsClient )
					return;

				Event.Run( PBEvent.Round.End, GetWinner() );
				RPC.OnRoundStateChanged( RoundState.End, GetWinner() );

				UntilStateEnds = Game.Current.Settings.EndDuration;

				break;
			}
		}
	}

	public void RoundStateFinish()
	{
		Host.AssertServer();

		switch ( RoundState )
		{
			case RoundState.Freeze:
			{
				RoundState = RoundState.Play;

				break;
			}
			case RoundState.Play:
			{
				RoundState = RoundState.End;

				Team winner = GetWinner();

				if ( winner == Team.None )
					break;
				_ = winner == Team.Blue ? BlueScore++ : RedScore++;

				break;
			}
			case RoundState.Bomb:
			{
				RoundState = RoundState.End;

				Team winner = GetWinner();
				_ = winner == Team.Blue ? BlueScore++ : RedScore++;

				break;
			}
			case RoundState.End:
			{
				Round++;

				if ( BlueScore == ToWinScore || RedScore == ToWinScore || Round > Game.Current.Settings.RoundLimit )
				{
					Bomb?.Delete();
					Bomb = null;
					Game.Current.ChangeState( new MapSelectState() );

					return;
				}

				RoundState = RoundState.Freeze;

				AliveBlue = 0;
				AliveRed = 0;

				break;
			}
		}

		RoundStateStart();
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
			return BombEnabled ? Team.Blue : Team.None;

		if ( AliveBlue > AliveRed )
			return Team.Blue;
		else
			return Team.Red;
	}

	private void CheckRoundOver()
	{
		if ( RoundState == RoundState.Play )
		{
			if ( AliveBlue == 0 || AliveRed == 0 )
				RoundStateFinish();
		}
		else if ( RoundState == RoundState.Bomb )
		{
			if ( AliveBlue == 0 )
				RoundStateFinish();
		}
	}

	private void TeamBalance()
	{
		var teamBlue = Team.Blue.GetAll();
		var teamRed = Team.Red.GetAll();

		int diff = Math.Abs( teamBlue.Count() - teamRed.Count() ) >> 1;

		if ( diff <= 0 )
			return;

		Notification.Create( To.Everyone, "Teams have been Auto-Balanced!", 3 );

		Team teamLess = teamBlue.Count() > teamRed.Count() ? Team.Red : Team.Blue;
		var teamMore = teamLess == Team.Blue ? teamRed : teamBlue;

		foreach ( var player in teamMore )
		{
			player.SetTeam( teamLess );

			if ( --diff == 0 )
				break;
		}
	}
}
