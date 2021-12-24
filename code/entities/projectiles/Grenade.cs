using Sandbox;
using System;

namespace PaintBall
{
	[Library]
	public partial class Grenade : Projectile
	{
		public float Bounciness { get; set; } = 0.3f;
		public override bool ExplodeOnDestroy => true;
		public override float LifeTime => 5f;
		private const float _pi4 = (float)Math.PI / 4;
		private static readonly Vector3[] s_directions =
		{
			Vector3.Up,
			Vector3.Left,
			Vector3.Right,
			Vector3.Backward,
			Vector3.Forward,
			new Vector3(_pi4,-_pi4, 0),
			new Vector3(_pi4,_pi4, 0),
			new Vector3(-_pi4,_pi4, 0),
			new Vector3(-_pi4,-_pi4, 0),
			new Vector3(_pi4,-_pi4, _pi4/2),
			new Vector3(_pi4,_pi4, _pi4/2),
			new Vector3(-_pi4,_pi4, _pi4/2),
			new Vector3(-_pi4,-_pi4, _pi4/2),
			Vector3.Left + new Vector3(0, 0, _pi4/4),
			Vector3.Right + new Vector3(0, 0, _pi4/4),
			Vector3.Backward + new Vector3(0, 0, _pi4/4),
			Vector3.Forward + new Vector3(0, 0, _pi4/4)
		};

		protected override bool HasHitTarget( TraceResult trace )
		{
			if ( trace.Hit )
			{
				var reflect = Vector3.Reflect( Velocity.Normal, trace.Normal );

				GravityModifier = 0f;
				Velocity = reflect * Velocity.Length * Bounciness;

				return false;
			}

			return base.HasHitTarget( trace );
		}

		protected override void OnExplode()
		{
			if ( Owner is not Player owner )
				return;

			if ( IsClientOnly && !IsLocalPawn )
				return;

			foreach ( var direction in s_directions )
			{
				var projectile = new Projectile()
				{
					Owner = owner,
					Team = owner.Team,
					FollowEffect = $"particles/{owner.Team.GetString()}_glow.vpcf",
					HitSound = HitSound,
					IgnoreTag = IgnoreTag,
					Scale = 0.25f,
					Radius = Radius,
					Gravity = 0f,
					Simulator = owner.Projectiles,
					Model = $"models/{owner.Team.GetString()}_ball/ball.vmdl"
				};

				var velocity = direction * 2500f;

				projectile.Initialize( Position, velocity, Callback );
			}
		}
	}
}
