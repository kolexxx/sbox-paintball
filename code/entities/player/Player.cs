using Sandbox;

namespace PaintBall
{
	public partial class Player : Sandbox.Player
	{
		[Net, Change] public Team Team { get; set; }
		[Net] public TimeSince TimeSinceSpawned { get; private set; }
		public ProjectileSimulator Projectiles { get; set; }
		private DamageInfo LastDamageInfo { get; set; }

		public Player()
		{
			Inventory = new Inventory( this );
			Projectiles = new( this );
			EnableTouch = true;
		}

		public override void Respawn()
		{
			TimeSinceSpawned = 0f;

			RemoveCorpse();

			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new CustomWalkController();

			Animator = new StandardPlayerAnimator();

			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = false;

			TimeSinceSpawned = 0f;
			RenderColor = Team.GetColor();

			base.Respawn();

			Game.Instance.CurrentGameState.OnPlayerSpawned( this );
		}

		public override void Simulate( Client cl )
		{
			Projectiles.Simulate();

			var controller = GetActiveController();

			SimulateActiveChild( cl, ActiveChild );

			if ( Input.ActiveChild != null )
				ActiveChild = Input.ActiveChild;

			if ( LifeState != LifeState.Alive )
			{
				ChangeSpectateCamera();

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

		public void SetTeam( Team newTeam )
		{
			Team oldTeam = Team;
			Tags.Remove( $"{oldTeam.GetString()}player" );
			Team = newTeam;
			Tags.Add( $"{newTeam.GetString()}player" );
			Client.SetInt( "team", (int)newTeam );

			Hud.OnTeamChanged( To.Everyone, Client, newTeam );

			Game.Instance.CurrentGameState.OnPlayerChangedTeam( this, oldTeam, newTeam );
		}

		public void OnTeamChanged( Team oldTeam, Team newTeam )
		{
			if ( IsLocalPawn )
			{
				Local.Hud.RemoveClass( oldTeam.GetString() );
				Local.Hud.AddClass( newTeam.GetString() );
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

			BecomeRagdollOnClient( LastDamageInfo.Force, GetHitboxBone( LastDamageInfo.HitboxIndex ) );

			RemoveAllDecals();

			var attacker = LastAttacker;

			if ( attacker.IsValid() )
			{
				if ( attacker is Player killer )
					killer?.OnPlayerKill();

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
			if ( TimeSinceSpawned < 1f )
				return;

			LastDamageInfo = info;

			base.TakeDamage( info );
		}
	}
}
