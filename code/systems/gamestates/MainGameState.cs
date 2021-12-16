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
		private int _roundLimit => 12;
		private int _toWinScore => 7;
		private int _round = 0;
		private readonly float[] _roundStateDuration = { 5f, 60f, 5f };


		public enum RoundState
		{
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
		}

		public override void OnPlayerKilled( Player player, Entity attacker, DamageInfo info )
		{
			AdjustTeam( player.Team, -1 );

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
				(GameInfo.Instance.Left.GetChild( 0 ) as Sandbox.UI.Label).Text = "0";
				(GameInfo.Instance.Right.GetChild( 0 ) as Sandbox.UI.Label).Text = "0";
			}
		}

		private void RoundStateStart()
		{
			switch ( CurrentRoundState )
			{
				case RoundState.Freeze:

					FreezeTime = 0;

					Game.Instance.CleanUp();

					foreach ( var player in Players )
					{
						if ( !player.IsValid() || player.Team == Team.None )
							continue;

						player.Inventory.DeleteContents();

						player.Respawn();
					}

					int diff = Math.Abs( AliveBlue - AliveRed );

					if ( Math.Abs( AliveBlue - AliveRed ) > 1 )
						TeamBalance( diff );

					break;

				case RoundState.Play:

					Audio.PlayAll( "prepare" );

					break;

				case RoundState.End:

					break;
			}

			StateEndTime = _roundStateDuration[(int)CurrentRoundState] + Time.Now;

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

					Audio.PlayAll( $"{winner}win".ToLower() );

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
				AliveBlue += num;
			else
				AliveRed += num;
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
			(GameInfo.Instance.Left.GetChild( 0 ) as Sandbox.UI.Label).Text = AliveBlue.ToString();
		}

		private void OnAliveRedChanged()
		{
			(GameInfo.Instance.Right.GetChild( 0 ) as Sandbox.UI.Label).Text = AliveRed.ToString();
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
			if ( oldState == RoundState.End )
				Hud.Reset();
		}

		private void TeamBalance( int diff )
		{
			Team team = AliveBlue > AliveRed ? Team.Red : Team.Blue;

			var players = team.GetAll();

			foreach ( var player in players )
			{
				player.SetTeam( team );

				if ( --diff == 0 )
					break;
			}
		}
	}
}
