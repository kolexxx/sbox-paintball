using Sandbox;
using System;
using System.Linq;

namespace PaintBall
{
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
		[Net, Change] public int AliveBlue { get; private set; } = 0;
		[Net, Change] public int AliveRed { get; private set; } = 0;
		[Net] public int BlueScore { get; private set; } = 0;
		[Net] public int RedScore { get; private set; } = 0;
		[Net] public PlantedBomb Bomb { get; set; }
		[Net, Change] public RoundState CurrentRoundState { get; private set; }
		public override bool UpdateTimer => CurrentRoundState != RoundState.End;
		private bool _firstBlood = false;
		private int _roundLimit => 12;
		private int _toWinScore => 7;
		private int _round = 0;

		public enum RoundState : byte
		{
			None,
			Freeze,
			Play,
			End
		}

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

			switch ( CurrentRoundState )
			{
				case RoundState.Freeze:

					break;

				case RoundState.Play:

					if ( Host.IsServer && (AliveBlue == 0 || (!Bomb.IsValid() && AliveRed == 0)) )
						RoundStateFinish();

					break;

				case RoundState.End:

					break;
			}

			Bomb?.Tick();
		}

		public override void Start()
		{
			base.Start();

			foreach ( var player in Players )
				player.Reset();

			CurrentRoundState = RoundState.Freeze;

			if ( Host.IsServer )
				RoundStateStart();
		}

		private void RoundStateStart()
		{
			switch ( CurrentRoundState )
			{
				case RoundState.Freeze:

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
						if ( --index == 0 )
							player.Inventory.Add( new Bomb() );
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

				case RoundState.End:

					Event.Run( PBEvent.Round.End, GetWinner() );

					StateEndTime = EndDuration + Time.Now;

					break;
			}

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

					Audio.AnnounceAll( $"{winner.GetString()}win", Audio.Priority.High );

					_ = winner == Team.Blue ? BlueScore++ : RedScore++;

					break;

				case RoundState.End:

					_round++;

					if ( BlueScore == _toWinScore || RedScore == _toWinScore || _round == _roundLimit )
					{
						Game.Current.ChangeState( new GameFinishedState() );

						return;
					}

					CurrentRoundState = RoundState.Freeze;

					AliveBlue = 0;
					AliveRed = 0;

					break;
			}

			RoundStateStart();
		}

		public void BombPlanted()
		{
			if ( CurrentRoundState == RoundState.Play )
			{
				StateEndTime = Time.Now + 30f;
				NextSecondTime = 0f;
			}
		}

		public void BombExplode()
		{
			if ( CurrentRoundState == RoundState.Play )
			{
				RoundStateFinish();
			}
		}

		public void BombDefused()
		{
			if ( CurrentRoundState == RoundState.Play )
			{
				RoundStateFinish();
			}
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
			if ( Bomb.IsValid() )
			{
				if ( Bomb.Defused )
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
}
