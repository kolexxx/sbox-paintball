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
		if ( Host.IsClient )
		{
			if ( Client.All.Count > 1 )
				UI.Notification.Create( $"Starting in {TimeLeftSeconds}" );
			else
				UI.Notification.Create( "Waiting for players..." );
		}

		base.OnSecond();
	}

	public override void Tick()
	{
		base.Tick();

		if ( Host.IsServer && Client.All.Count == 1 )
			UntilStateEnds = StateDuration;
	}

	public override void Start()
	{
		base.Start();

		if ( !Host.IsServer )
			return;

		if ( Client.All.Count > 1 )
			UntilStateEnds = StateDuration;

		foreach ( var client in Client.All )
		{
			var player = client.Pawn as Player;
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
