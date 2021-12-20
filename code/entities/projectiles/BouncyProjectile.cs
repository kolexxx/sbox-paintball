using Sandbox;
using System;

namespace PaintBall
{
	public class BouncyProjectile : Projectile
	{
		public float Bounciness { get; set; } = 0.8f;
		public override float LifeTime => 5f;

		protected override bool HasHitTarget( TraceResult trace )
		{
			if ( trace.Hit )
			{
				if ( trace.Entity is not WorldEntity )
					return true;
				else if ( trace.Normal.z <= 1f && trace.Normal.z >= Math.PI / 4 )
					return true;
				
				var reflect = Vector3.Reflect( Velocity.Normal, trace.Normal );

				GravityModifier = 0f;
				Velocity = reflect * Velocity.Length * Bounciness;

				return false;
			}

			return base.HasHitTarget( trace );
		}
	}
}
