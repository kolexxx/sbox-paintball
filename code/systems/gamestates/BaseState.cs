using Sandbox;
using System.Collections.Generic;

namespace Paintball;

public abstract partial class BaseState : BaseNetworkable
{
	[Net, Change] public int AliveBlue { get; protected set; } = 0;
	[Net, Change] public int AliveRed { get; protected set; } = 0;
	[Net, Change] public int BlueScore { get; protected set; } = 0;
	[Net, Change] public int RedScore { get; protected set; } = 0;
	[Net] public TimeUntil UntilStateEnds { get; set; }
	public virtual bool CanBuy => true;
	public virtual bool CanPlayerSuicide => false;
	public virtual int StateDuration => 0;
	public virtual string Name => GetType().Name;
	public virtual bool UpdateTimer => false;
	public int TimeLeftSeconds => UntilStateEnds.Relative.CeilToInt();
	protected RealTimeUntil NextSecondTime { get; set; } = 0f;

	public BaseState() { }

	public virtual void OnPlayerJoin( Player player )
	{
		Host.AssertServer();

		if ( player.Client.IsBot )
		{
			if ( Team.Blue.GetCount() >= Team.Red.GetCount() )
				player.SetTeam( Team.Red );
			else
				player.SetTeam( Team.Blue );
		}
		else
		{
			// player.MakeSpectator();
		}
	}

	public virtual void OnPlayerLeave( Player player )
	{
		Host.AssertServer();

		if ( player.Alive() )
			AdjustTeam( player.Team, -1 );
	}

	public virtual void OnPlayerSpawned( Player player )
	{
		Host.AssertServer();

		AdjustTeam( player.Team, 1 );

		Game.Current.MoveToSpawnpoint( player );
	}

	public virtual void OnPlayerKilled( Player player )
	{
		Host.AssertServer();

		AdjustTeam( player.Team, -1 );
	}

	public virtual void OnPlayerChangedTeam( Player player, Team oldTeam )
	{
		Host.AssertServer();

		if ( player.Alive() )
		{
			AdjustTeam( oldTeam, -1 );

			player.TakeDamage( DamageInfo.Generic( float.MaxValue ) );
		}

		if ( player.Team == Team.None )
		{
			player.MakeSpectator();

			return;
		}
		else
		{
			player.Respawn();
		}
	}

	public virtual void OnSecond()
	{
		if ( UntilStateEnds )
			TimeUp();
	}

	public virtual void Tick()
	{
		if ( NextSecondTime <= 0 )
		{
			OnSecond();
			NextSecondTime = 1f;
		}
	}

	public virtual void Start()
	{
		if ( Host.IsServer && StateDuration > 0 )
			UntilStateEnds = StateDuration;
	}

	public virtual void Finish() { }

	public virtual void TimeUp() { }

	protected void AdjustTeam( Team team, int amount )
	{
		if ( team == Team.None )
			return;

		if ( team == Team.Blue )
			AliveBlue += amount;
		else
			AliveRed += amount;
	}

	#region callbacks;
	private void OnAliveBlueChanged()
	{
		UI.RoundInfo.Instance.AliveBlue.Text = AliveBlue.ToString();
	}

	private void OnAliveRedChanged()
	{
		UI.RoundInfo.Instance.AliveRed.Text = AliveRed.ToString();
	}

	private void OnBlueScoreChanged()
	{
		UI.RoundInfo.Instance.BlueScore.Text = BlueScore.ToString();
	}

	private void OnRedScoreChanged()
	{
		UI.RoundInfo.Instance.RedScore.Text = RedScore.ToString();
	}
	#endregion
}
