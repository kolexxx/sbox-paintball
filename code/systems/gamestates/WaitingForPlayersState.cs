using Paintball.UI;
using Sandbox;

namespace Paintball;

public partial class WaitingForPlayersState : BaseState
{
	public override int StateDuration => 10;

	public override void OnPlayerJoin( Player player )
	{
		base.OnPlayerJoin( player );

		UntilStateEnds = StateDuration;
		NextSecondTime = 0f;
	}

	public override void OnPlayerKilled( Player player, Entity attacker, DamageInfo info )
	{
		base.OnPlayerKilled( player, attacker, info );

		player.Respawn();

		Game.Current.MoveToSpawnpoint( player );
	}

	public override void OnPlayerSpawned( Player player )
	{
		player.Inventory.Add( (Rand.Int( 1, 2 ) == 1 ? new SMG() : new Shotgun()), true );
		player.Inventory.Add( new Pistol() );
		player.Inventory.Add( new Knife() );

		base.OnPlayerSpawned( player );
	}

	public override void OnSecond()
	{
		base.OnSecond();

		if ( Host.IsServer )
		{
			if ( Players.Count > 1 )
				Notification.Create( $"Starting in {TimeLeftSeconds}" );
			else
				Notification.Create( "Waiting for players..." );
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( Host.IsServer )
		{
			if ( Players.Count > 1 )
			{
				if ( UntilStateEnds )
					Game.Current.ChangeState( new GameplayState() );
			}
			else
			{
				UntilStateEnds = 0;
			}
		}
	}

	public override void Start()
	{
		base.Start();

		if ( Players.Count > 1 )
			UntilStateEnds = StateDuration;

		foreach ( var player in Players )
		{
			if ( player.Team == Team.None )
				continue;

			player.Reset();

			player.Respawn();
		}
	}
}
