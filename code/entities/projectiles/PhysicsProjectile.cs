using Hammer;
using Sandbox;
using System;

namespace PaintBall
{
	[Skip]
	public partial class PhysicsProjectile : ModelEntity, IProjectile
	{
		public RealTimeUntil DestroyTime { get; set; }
		public Entity Origin { get; set; }
		public string ModelPath => "models/grenade/grenade_spent.vmdl";
		public Team Team { get; set; }
		public float LifeTime => 3f;

		private static float _sqrt2over2 = (float)Math.Sqrt( 2 ) / 2f;
		// ATTENTION! THIS IS REALLY SHIT! THIS CAN BE DONE USING A SIMPLE FOR LOOP WITH ORIENTATION!
		private static readonly Vector3[] s_directions =
		{
			Vector3.Left,
			Vector3.Right,
			Vector3.Backward,
			Vector3.Forward,
			new Vector3(_sqrt2over2,-_sqrt2over2, 0),
			new Vector3(_sqrt2over2,_sqrt2over2, 0),
			new Vector3(-_sqrt2over2,_sqrt2over2, 0),
			new Vector3(-_sqrt2over2,-_sqrt2over2, 0),
			new Vector3((float)Math.Sin(Math.PI/8),-(float)Math.Cos(Math.PI/8), 0),
			new Vector3((float)Math.Sin(Math.PI/8),(float)Math.Cos(Math.PI/8), 0),
			new Vector3(-(float)Math.Sin(Math.PI/8),(float)Math.Cos(Math.PI/8), 0),
			new Vector3(-(float)Math.Sin(Math.PI/8),-(float)Math.Cos(Math.PI/8), 0),
			new Vector3((float)Math.Cos(Math.PI/8),-(float)Math.Sin(Math.PI/8), 0),
			new Vector3((float)Math.Cos(Math.PI/8),(float)Math.Sin(Math.PI/8), 0),
			new Vector3(-(float)Math.Cos(Math.PI/8),(float)Math.Sin(Math.PI/8), 0),
			new Vector3(-(float)Math.Cos(Math.PI/8),-(float)Math.Sin(Math.PI/8), 0),
			new Vector3((float)Math.Sin(Math.PI/8),-(float)Math.Cos(Math.PI/8), _sqrt2over2/4),
			new Vector3((float)Math.Sin(Math.PI/8),(float)Math.Cos(Math.PI/8), _sqrt2over2/4),
			new Vector3(-(float)Math.Sin(Math.PI/8),(float)Math.Cos(Math.PI/8), _sqrt2over2/4),
			new Vector3(-(float)Math.Sin(Math.PI/8),-(float)Math.Cos(Math.PI/8), _sqrt2over2/4),
			new Vector3((float)Math.Cos(Math.PI/8),-(float)Math.Sin(Math.PI/8), _sqrt2over2/4),
			new Vector3((float)Math.Cos(Math.PI/8),(float)Math.Sin(Math.PI/8), _sqrt2over2/4),
			new Vector3(-(float)Math.Cos(Math.PI/8),(float)Math.Sin(Math.PI/8), _sqrt2over2/4),
			new Vector3(-(float)Math.Cos(Math.PI/8),-(float)Math.Sin(Math.PI/8), _sqrt2over2/4),
			new Vector3(_sqrt2over2,-_sqrt2over2, _sqrt2over2/4),
			new Vector3(_sqrt2over2,_sqrt2over2, _sqrt2over2/4),
			new Vector3(-_sqrt2over2,_sqrt2over2, _sqrt2over2/4),
			new Vector3(-_sqrt2over2,-_sqrt2over2, _sqrt2over2/4),
			Vector3.Left + new Vector3(0, 0, _sqrt2over2/4),
			Vector3.Right + new Vector3(0, 0, _sqrt2over2/4),
			Vector3.Backward + new Vector3(0, 0, _sqrt2over2/4),
			Vector3.Forward + new Vector3(0, 0, _sqrt2over2/4)
		};

		public override void Spawn()
		{
			base.Spawn();

			DestroyTime = LifeTime;
			SetModel( ModelPath );
			MoveType = MoveType.Physics;
			UsePhysicsCollision = true;
			SetInteractsAs( CollisionLayer.All );
			SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
			SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );
			Tags.Add( "grenade" );
		}

		[Event.Tick.Server]
		public void ServerTick()
		{
			if ( DestroyTime )
			{
				OnExplode();
				Delete();
			}
		}

		protected void OnExplode()
		{
			if ( Owner is not Player owner )
				return;

			foreach ( var direction in s_directions )
			{
				var projectile = new BaseProjectile()
				{
					Owner = owner,
					Team = Team,
					FollowEffect = $"particles/{owner.Team.GetString()}_glow.vpcf",
					HitSound = "impact",
					Scale = 0.25f,
					Gravity = 0f,
					ModelPath = $"models/{owner.Team.GetString()}_ball/ball.vmdl",
					IsServerOnly = true,
					Origin = Origin
				};

				var velocity = direction * 1000f;

				projectile.Initialize( PhysicsBody.MassCenter, velocity, 4f);
			}
		}
	}
}
