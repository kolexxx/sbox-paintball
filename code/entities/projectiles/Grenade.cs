using Hammer;
using Sandbox;

namespace Paintball;

[Skip]
public partial class Grenade : ModelEntity, ITeamEntity
{
	public RealTimeUntil DestroyTime { get; set; }
	public int ProjectilesPer360 { get; set; } = 20;
	public Entity Origin { get; set; }
	public string ModelPath => "models/grenade/grenade_spent.vmdl";
	public Team Team { get; set; }
	public float LifeTime => 3f;

	public override void Spawn()
	{
		base.Spawn();

		DestroyTime = LifeTime;
		SetModel( ModelPath );
		MoveType = MoveType.Physics;
		UsePhysicsCollision = true;
		SetInteractsAs( CollisionLayer.Solid );
		SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		SetInteractsExclude( CollisionLayer.Player );
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

		for ( float pitch = 0; pitch >= -45; pitch -= 15)
		{
			for ( float yaw = 0; yaw <= 360; yaw += 360 / ProjectilesPer360 )
			{
				var angles = new Angles( pitch, yaw, 0 );

				var projectile = new BaseProjectile()
				{
					Owner = owner,
					Team = Team,
					FollowEffect = $"particles/{owner.Team.GetString()}_glow.vpcf",
					HitSound = "impact",
					Scale = 0.25f,
					Gravity = 10f,
					ModelPath = $"models/{owner.Team.GetString()}_ball/ball.vmdl",
					IsServerOnly = true,
					Origin = Origin
				};

				var velocity = Rotation.From( angles ).Forward * 1000f;

				projectile.Initialize( PhysicsBody.MassCenter, velocity, 4f );
			}
		}
	}
}
