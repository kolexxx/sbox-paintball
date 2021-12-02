using Sandbox;
using System;

namespace PaintBall
{
	public partial class WaitingForPlayersState : BaseState
	{
		public override int StateDuration => 10;

		public override void OnPlayerJoin( Player player )
		{
			base.OnPlayerJoin( player );

			player.SetTeam( (Team)Rand.Int( 1, 2 ) );
			player.Respawn();

			StateEndTime = StateDuration + Time.Now;
			NextSecondTime = 0f;
		}

		public override void OnPlayerKilled( Player player, Entity attacker, DamageInfo info )
		{
			base.OnPlayerKilled( player, attacker, info );

			player.Respawn();
		}

		public override void OnPlayerSpawned( Player player )
		{
			player.Inventory.Add( new SMG(), true );
			player.Inventory.Add( new Pistol() );
		}

		public override void OnSecond()
		{
			base.OnSecond();

			if ( Host.IsServer )
			{
				if ( Players.Count > 1 )
					Hud.UpdateCrosshairMessage( $"Starting in {TimeLeftSeconds}" );
				else
					Hud.UpdateCrosshairMessage( "Waiting for players..." );
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
						Game.Instance.ChangeState( new MainGameState() );
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
				player.Reset();

				player.SetTeam( (Team)Rand.Int( 1, 2 ) );

				player.Respawn();
			}
		}
	}
}
