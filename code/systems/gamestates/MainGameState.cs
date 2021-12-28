using Sandbox;
using System;
using System.Linq;

namespace PaintBall
{
	public partial class MainGameState : BaseState
	{
		[Net, Change] public int AliveBlue { get; private set; } = 0;
		[Net, Change] public int AliveRed { get; private set; } = 0;
		[Net, Change] public int BlueScore { get; private set; } = 0;
		[Net, Change] public int RedScore { get; private set; } = 0;
		[Net, Change] public RoundState CurrentRoundState { get; private set; }
		public override bool UpdateTimer => CurrentRoundState != RoundState.End;
		private bool _firstBlood = false;
		private int _roundLimit => 12;
		private int _toWinScore => 7;
		private int _round = 0;
		private readonly float[] _roundStateDuration = { 5f, 60f, 5f };

		public enum RoundState
		{
			None,
			Freeze,
			Play,
			End
		}

		public override void OnPlayerJoin( Player player )
		{
			base.OnPlayerJoin( player );

			if ( player.Client.IsBot )
			{
				if ( Team.Blue.GetCount() >= Team.Red.GetCount() )
					player.SetTeam( Team.Red );
				else
					player.SetTeam( Team.Blue );
			}
			else
			{
				player.MakeSpectator();
			}

			Game.Instance.MoveToSpawnpoint( player );
		}

		public override void OnPlayerLeave( Player player )
		{
			base.OnPlayerLeave( player );

			if ( player.LifeState != LifeState.Dead )
				AdjustTeam( player.Team, -1 );
		}

		public override void OnPlayerSpawned( Player player )
		{
			Host.AssertServer();

			AdjustTeam( player.Team, 1 );

			player.Inventory.Add( (Rand.Int( 1, 2 ) == 1 ? new SMG() : new Shotgun()), true );
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

		public override void OnPlayerChangedTeam( Player player, Team oldTeam, Team newTeam )
		{
			if ( player.LifeState != LifeState.Dead )
			{
				AdjustTeam( oldTeam, -1 );
				AdjustTeam( newTeam, 1 );
			}
		}

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

			switch ( CurrentRoundState )
			{
				case RoundState.Freeze:

					break;

				case RoundState.Play:

					if ( Host.IsServer && (AliveBlue == 0 || AliveRed == 0) )
					{
						RoundStateFinish();

						return;
					}

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

			CurrentRoundState = RoundState.Freeze; // No need for this.

			if ( Host.IsServer )
				RoundStateStart();
		}

		public override void Finish()
		{
			base.Finish();

			if ( Host.IsClient )
			{
				Hud.UpdateTeamScore( Team.Blue );
				Hud.UpdateTeamScore( Team.Red );
				(RoundInfo.Instance.Left.GetChild( 0 ) as Sandbox.UI.Label).Text = "0";
				(RoundInfo.Instance.Right.GetChild( 0 ) as Sandbox.UI.Label).Text = "0";
			}
		}

		private void RoundStateStart()
		{
			switch ( CurrentRoundState )
			{
				case RoundState.Freeze:

					_firstBlood = false;
					FreezeTime = 0;

					TeamBalance();

					Game.Instance.CleanUp();

					foreach ( var player in Players )
					{
						if ( !player.IsValid() || player.Team == Team.None )
							continue;

						player.Inventory.DeleteContents();

						player.Respawn();
					}

					Event.Run( PBEvent.Round.New );

					break;

				case RoundState.Play:

					Audio.AnnounceAll( "prepare", Audio.Priority.High );

					Event.Run( PBEvent.Round.Start );

					break;

				case RoundState.End:

					Event.Run( PBEvent.Round.End );

					break;
			}

			StateEndTime = _roundStateDuration[(int)CurrentRoundState - 1] + Time.Now;

			// Call OnSecond() as soon as RoundState starts
			NextSecondTime = 0f;
		}

		private void RoundStateFinish()
		{
			switch ( CurrentRoundState )
			{
				case RoundState.Freeze:

					CurrentRoundState = RoundState.Play;

					break;

				case RoundState.Play:

					CurrentRoundState = RoundState.End;

					Team winner = GetWinner();

					Hud.UpdateCrosshairMessage( winner + " wins!" );

					Audio.AnnounceAll( $"{winner.GetString()}win", Audio.Priority.High );

					_ = winner == Team.Blue ? BlueScore++ : RedScore++;

					break;

				case RoundState.End:

					_round++;

					if ( BlueScore == _toWinScore || RedScore == _toWinScore || _round == _roundLimit )
					{
						Game.Instance.ChangeState( new GameFinishedState() );

						return;
					}

					CurrentRoundState = RoundState.Freeze;

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
			if ( AliveBlue != 0 && AliveRed != 0 )
				return Team.Blue; // Blue always wins

			if ( AliveBlue > AliveRed )
				return Team.Blue;
			else
				return Team.Red;
		}

		private void OnAliveBlueChanged()
		{
			(RoundInfo.Instance.Left.GetChild( 0 ) as Sandbox.UI.Label).Text = AliveBlue.ToString();
		}

		private void OnAliveRedChanged()
		{
			(RoundInfo.Instance.Right.GetChild( 0 ) as Sandbox.UI.Label).Text = AliveRed.ToString();
		}

		private void OnBlueScoreChanged()
		{
			Hud.UpdateTeamScore( Team.Blue, BlueScore.ToString() );
		}

		private void OnRedScoreChanged()
		{
			Hud.UpdateTeamScore( Team.Red, RedScore.ToString() );
		}

		private void OnCurrentRoundStateChanged( RoundState oldState, RoundState newState )
		{
			switch ( newState )
			{
				case RoundState.Freeze:

					Event.Run( PBEvent.Round.New );

					return;

				case RoundState.Play:

					Event.Run( PBEvent.Round.Start );

					return;

				case RoundState.End:

					Event.Run( PBEvent.Round.End );

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
}
