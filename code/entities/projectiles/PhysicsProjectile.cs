using Sandbox;



namespace PaintBall
{
	[Library]
	public class PhysicsProjectile : ModelEntity
	{
		public string FollowEffect { get; set; }
		public float? LifeTime { get; set; } = 10f;
		public Team Team { get; set; }
		protected Particles Follower { get; set; }
		protected Particles Trail { get; set; }
		private RealTimeUntil DestroyTime { get; set; }

		public void Initialize()
		{
			if ( LifeTime.HasValue )
				DestroyTime = LifeTime.Value;

			Transmit = TransmitType.Always;

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			using ( Prediction.Off() )
			{
				CreateEffects();
			}
		}

		[Event.Tick.Server]
		protected void ServerTick()
		{
			if ( DestroyTime )
			{
				Kill();
			}
		}

		protected void Kill()
		{
			// Fire paintballs in all directions

			Delete();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			RemoveEffects();
		}

		private void CreateEffects()
		{
			if ( string.IsNullOrEmpty( FollowEffect ) )
				Follower = Particles.Create( FollowEffect );
		}

		private void RemoveEffects()
		{
			Follower?.Destroy( true );
			Trail?.Destroy();
			Trail = null;
		}
	}
}
