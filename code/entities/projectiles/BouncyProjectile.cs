using Sandbox;
using System;

namespace Paintball;

[Hammer.Skip]
public sealed partial class BouncyProjectile : BaseProjectile
{
	public float Bounciness { get; set; } = 0.6f;
	public override float LifeTime => 5f;

	protected override bool HasHitTarget( ref TraceResult trace )
	{
		if ( trace.Hit )
		{
			if ( trace.Entity is not WorldEntity )
				return true;
			else if ( trace.Normal.z <= 1f && trace.Normal.z >= 0.70710678118f )
				return true;

			var reflect = Vector3.Reflect( Velocity.Normal, trace.Normal );

			GravityModifier = 0f;
			Velocity = reflect * Velocity.Length * Bounciness;

			return false;
		}

		return base.HasHitTarget( ref trace );
	}
}
