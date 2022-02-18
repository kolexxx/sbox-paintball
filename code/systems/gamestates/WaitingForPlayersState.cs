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

	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );

		player.Respawn();

		Game.Current.MoveToSpawnpoint( player );
	}

	public override void OnPlayerSpawned( Player player )
	{
		player.Inventory.Add( new Pistol() );
		player.Inventory.Add( new Knife() );

		base.OnPlayerSpawned( player );
	}

	public override void OnSecond()
	{
		if ( Host.IsServer )
		{
			if ( Players.Count > 1 )
				UI.Notification.Create( $"Starting in {TimeLeftSeconds}" );
			else
				UI.Notification.Create( "Waiting for players..." );
		}

		base.OnSecond();
	}

	public override void Tick()
	{
		base.Tick();

		if ( Host.IsServer && Players.Count == 1 )
			UntilStateEnds = StateDuration;
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

	public override void TimeUp()
	{
		base.TimeUp();

		Game.Current.ChangeState( new GameplayState( Map.Bombsites.Count > 0 ) );
	}
}
