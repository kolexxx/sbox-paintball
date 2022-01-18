using Sandbox;
using System;
using System.Linq;

namespace PaintBall;

public enum RoundState : byte
{
	None,
	Freeze,
	Play,
	Bomb,
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

	public PlantedBomb Bomb { get; set; }
	[Net] public Player Defuser { get; set; }
	[Net] public Player Planter { get; set; }
	[Net, Change( nameof( OnDisabled ) )] public bool Disabled { get; set; }
	[Net, Change] public int AliveBlue { get; private set; } = 0;
	[Net, Change] public int AliveRed { get; private set; } = 0;
	[Net] public int BlueScore { get; private set; } = 0;
	[Net] public int RedScore { get; private set; } = 0;
	[Net, Change] public RoundState RoundState { get; set; }
	public override bool UpdateTimer => RoundState != RoundState.End;
	private bool _firstBlood = false;
	private int _toWinScore => 7;
	private int _round = 0;

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		if ( player.LifeState != LifeState.Dead )
		{
			AdjustTeam( player.Team, -1 );
			player.Inventory.DropBomb();
		}
	}

	public override void OnPlayerSpawned( Player player )
	{
		Host.AssertServer();

		AdjustTeam( player.Team, 1 );

		// error
		// player.Inventory.Add( new ProjectileWeapon<Projectile>() );
		player.Inventory.Add( Rand.Int( 1, 2 ) == 1 ? new SMG() : new Shotgun(), true );
		player.Inventory.Add( new Pistol() );
		player.Inventory.Add( new Knife() );

		if ( Rand.Int( 1, 1 ) == 1 )
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
			if ( Time.Now >= StateEndTime )
				RoundStateFinish();

			TimeLeftSeconds = TimeLeft.CeilToInt();
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

				if ( AliveBlue == 0 || Bomb.Disabled )
					RoundStateFinish();

				Bomb.Tick();

				break;

			case RoundState.End:

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
				FreezeTime = 5f;

				TeamBalance();

				Game.Current.CleanUp();

				int index = Rand.Int( 1, Team.Red.GetCount() );

				foreach ( var player in Players )
				{
					if ( !player.IsValid() || player.Team == Team.None )
						continue;

					player.Inventory.DeleteContents();

					player.Respawn();
					if ( player.Team == Team.Red && --index == 0 )
					{
						player.Inventory.Add( new Bomb() );
					}
				}

				if ( BlueScore == _toWinScore - 1 || RedScore == _toWinScore - 1 )
					Notification.Create( To.Everyone, "Matchpoint!", FreezeDuration );

				Event.Run( PBEvent.Round.New );

				StateEndTime = FreezeDuration + Time.Now;

				break;

			case RoundState.Play:

				Audio.AnnounceAll( "prepare", Audio.Priority.High );

				Event.Run( PBEvent.Round.Start );

				StateEndTime = PlayDuration + Time.Now;

				break;

			case RoundState.Bomb:

				Event.Run( PBEvent.Round.Bomb.Planted, Bomb );

				StateEndTime = BombDuration + Time.Now;

				break;

			case RoundState.End:

				Event.Run( PBEvent.Round.End, GetWinner() );

				StateEndTime = EndDuration + Time.Now;

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
		if ( Bomb.IsValid() && Disabled )
		{
			if ( Defuser != null )
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

	private void OnRoundStateChanged( RoundState oldState, RoundState newState )
	{
		switch ( newState )
		{
			case RoundState.Freeze:

				Event.Run( PBEvent.Round.New );

				return;

			case RoundState.Play:

				Event.Run( PBEvent.Round.Start );

				return;

			case RoundState.Bomb:

				Event.Run( PBEvent.Round.Bomb.Planted, Bomb );

				return;

			case RoundState.End:

				Event.Run( PBEvent.Round.End, GetWinner() );

				return;
		}
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

	private void OnDisabled()
	{
		if ( Defuser != null )
			Event.Run( PBEvent.Round.Bomb.Defused, Bomb );
		else
			Event.Run( PBEvent.Round.Bomb.Explode, Bomb );
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
