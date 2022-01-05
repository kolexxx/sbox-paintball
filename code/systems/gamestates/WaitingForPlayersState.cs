using Sandbox;

namespace PaintBall
{
	public partial class WaitingForPlayersState : BaseState
	{
		public override int StateDuration => 10;

		public override void OnPlayerJoin( Player player )
		{
			base.OnPlayerJoin( player );

			StateEndTime = StateDuration + Time.Now;
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
			}
		}

		public override void Tick()
		{
			base.Tick();

			if ( Host.IsServer )
			{
				if ( Players.Count > 1 )
				{
					if ( TimeLeft <= 0 )
						Game.Current.ChangeState( new MainGameState() );
				}
				else
				{
					StateEndTime = Time.Now;
				}
			}
		}

		public override void Start()
		{
			base.Start();

			if ( Players.Count > 1 )
				StateEndTime = StateDuration + Time.Now;

			foreach ( var player in Players )
			{
				if ( player.Team == Team.None )
					continue;

				player.Reset();

				player.Respawn();
			}
		}
	}
}
