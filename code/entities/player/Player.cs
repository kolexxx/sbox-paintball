using Sandbox;
using Sandbox.UI;

namespace PaintBall
{
	public partial class Player : Sandbox.Player
	{
		[Net, Change] public Team Team { get; set; }
		public ProjectileSimulator Projectiles { get; set; }
		public TimeSince TimeSinceSpawned { get; private set; }
		private DamageInfo LastDamageInfo { get; set; }

		public Player()
		{
			Inventory = new BaseInventory( this );
			Projectiles = new( this );
			EnableTouch = true;
		}

		public override void Respawn()
		{
			Game.Instance.CurrentGameState.OnPlayerSpawned( this );

			Inventory.DeleteContents();
			Corpse?.Delete();

			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new CustomWalkController();

			Animator = new StandardPlayerAnimator();

			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Inventory.Add( new SMG(), true );
			Inventory.Add( new Pistol(), false );

			TimeSinceSpawned = 0f;
			RenderColor = Team.GetColor();

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			
			Projectiles.Simulate();

			var controller = GetActiveController();

			if ( controller is SpectatorController )
			{
				controller?.Simulate( cl, this, GetActiveAnimator() );
				return;
			}

			SimulateActiveChild( cl, ActiveChild );

			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( LifeState != LifeState.Alive )
			{
				return;
			}

			controller?.Simulate( cl, this, GetActiveAnimator() );
		}

		public void Reset()
		{
			Inventory.DeleteContents();

			Client.SetInt( "deaths", 0 );
			Client.SetInt( "kills", 0 );

			LastAttacker = null;
			LastDamageInfo = default;
		}

		public void SetTeam( Team team )
		{
			Tags.Remove( $"{Team.GetString()}player" );
			Team = team;
			Tags.Add( $"{Team.GetString()}player" );
			Client.SetInt( "team", (int)team );
		}

		public void OnTeamChanged( Team oldTeam, Team newTeam )
		{
			if ( IsLocalPawn )
			{
				Local.Hud.RemoveClass( oldTeam.GetString() );
				Local.Hud.AddClass( newTeam.GetString() );
				(Local.Hud.GetChild( 7 ).GetChild( 0 ) as Label).SetText( $"{newTeam}" );
			}
		}

		public override void Spawn()
		{
			LagCompensation = true;

			base.Spawn();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Inventory.DeleteContents();

			EnableDrawing = false;
			EnableAllCollisions = false;

			BecomeRagdollOnClient( LastDamageInfo.Force, GetHitboxBone( LastDamageInfo.HitboxIndex ) );

			RemoveAllDecals();

			Controller = new SpectatorController();


			var attacker = LastAttacker;

			if ( attacker.IsValid() )
			{
				if ( attacker is Player killer )
				{
					killer?.OnPlayerKill();
				}
				Game.Instance.CurrentGameState?.OnPlayerKilled( this, attacker, LastDamageInfo );
			}
			else
			{
				Game.Instance.CurrentGameState?.OnPlayerKilled( this, null, LastDamageInfo );
			}
		}

		protected void OnPlayerKill()
		{

		}

		public override void TakeDamage( DamageInfo info )
		{
			// Spawnprotection
			if ( TimeSinceSpawned < 0f )
				return;

			LastDamageInfo = info;

			base.TakeDamage( info );
		}
	}
}
