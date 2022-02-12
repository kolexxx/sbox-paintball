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
	public override bool CanBuy => !BuyTimeExpire;
	public RoundState RoundState { get; set; }
	public override bool UpdateTimer => RoundState != RoundState.End;
	private bool _firstBlood = false;
	private int _toWinScore => 9;
	private int _round = 0;

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		player.Inventory.DropBomb();
	}

	public override void OnPlayerSpawned( Player player )
	{
		base.OnPlayerSpawned( player );

		Host.AssertServer();

		if ( player.Inventory.HasFreeSlot( SlotType.Secondary ) )
			player.Inventory.Add( new Pistol() );
		if ( player.Inventory.HasFreeSlot( SlotType.Melee ) )
			player.Inventory.Add( new Knife() );
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
	}

	public override void OnPlayerChangedTeam( Player player, Team oldTeam, Team newTeam ) { }

	public override void Tick()
	{
		if ( !Host.IsServer )
			return;

		if ( Bomb.IsValid() )
			Bomb.Tick();

		if ( UntilStateEnds )
			TimeUp();

		switch ( RoundState )
		{
			case RoundState.Freeze:

				break;

			case RoundState.Play:

				if ( AliveBlue == 0 || AliveRed == 0 )
					RoundStateFinish();

				break;

			case RoundState.Bomb:

				if ( AliveBlue == 0 || Bomb.Disabled )
					RoundStateFinish();

				break;

			case RoundState.End:

				break;
		}
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

				Game.Current.Map.CleanUp();
				TeamBalance();

				int index = Rand.Int( 1, Team.Red.GetCount() );

				foreach ( var player in Players )
				{
					if ( !player.IsValid() || player.Team == Team.None )
						continue;

					player.Respawn();

					if ( player.Team == Team.Red && --index == 0 )
						player.Inventory.Add( new Bomb() );
				}

				if ( BlueScore == _toWinScore - 1 || RedScore == _toWinScore - 1 )
					Notification.Create( To.Everyone, "Matchpoint!", Game.Current.Settings.FreezeDuration );

				Event.Run( PBEvent.Round.New );
				RPC.OnRoundStateChanged( RoundState.Freeze );

				UntilStateEnds = Game.Current.Settings.FreezeDuration;
				BuyTimeExpire = 10;

				break;

			case RoundState.Play:

				Audio.AnnounceAll( "prepare", Audio.Priority.High );

				Event.Run( PBEvent.Round.Start );
				RPC.OnRoundStateChanged( RoundState.Play );

				UntilStateEnds = Game.Current.Settings.PlayDuration;

				break;

			case RoundState.Bomb:

				UntilStateEnds = Bomb.TimeUntilExplode;

				break;

			case RoundState.End:

				Event.Run( PBEvent.Round.End, GetWinner() );
				RPC.OnRoundStateChanged( RoundState.End, GetWinner() );

				UntilStateEnds = Game.Current.Settings.EndDuration;

				break;
		}
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

				if ( BlueScore == _toWinScore || RedScore == _toWinScore || _round >= Game.Current.Settings.RoundLimit )
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
