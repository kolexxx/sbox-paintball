using Sandbox;

namespace Paintball;

[Hammer.Skip]
public partial class BaseProjectile : ModelEntity, ITeamEntity
{
	[Net, Predicted] public Team Team { get; set; }
	public string FollowEffect => $"particles/{Team.GetTag()}_glow.vpcf";
	public string HitSound => "impact";
	public string ModelPath => "models/paintball/paintball.vmdl";
	public string TrailEffect { get; set; } = "";
	public bool IsServerOnly { get; set; }
	public string Attachment { get; set; } = null;
	public RealTimeUntil CanHitTime { get; set; } = 0.1f;
	public RealTimeUntil DestroyTime { get; set; }
	public float Gravity { get; set; } = 0f;
	public virtual float LifeTime => 10f;
	public Entity Origin { get; set; }
	public float Radius { get; set; } = 3f;
	public ProjectileSimulator Simulator { get; set; }
	public Vector3 StartPosition { get; private set; }
	protected Particles Follower { get; set; }
	protected float GravityModifier { get; set; }
	protected SceneObject ModelEntity { get; set; }
	protected Particles Trail { get; set; }

	public void Initialize( Vector3 start, Vector3 velocity, float scale = 0.25f )
	{
		DestroyTime = LifeTime;

		if ( Simulator != null && Simulator.IsValid() && !IsServerOnly )
		{
			Simulator?.Add( this );
			Owner = Simulator.Owner;
		}

		StartPosition = start;
		PhysicsEnabled = false;
		EnableDrawing = false;
		Velocity = velocity;
		Position = start;
		Scale = scale;

		Tags.Add( "projectile" );

		if ( IsClientOnly || IsServerOnly )
		{
			using ( Prediction.Off() )
			{
				CreateEffects();
			}
		}
	}

	public override void Spawn()
	{
		Predictable = true;

		base.Spawn();
	}

	public override void ClientSpawn()
	{
		// We only want to create effects if we don't have a client proxy.
		if ( !HasClientProxy() )
			CreateEffects();

		base.ClientSpawn();
	}

	[ClientRpc]
	protected void CreateEffects()
	{
		if ( !string.IsNullOrEmpty( TrailEffect ) )
		{
			Trail = Particles.Create( TrailEffect, this );

			if ( !string.IsNullOrEmpty( Attachment ) )
				Trail.SetEntityAttachment( 0, this, Attachment );
			else
				Trail.SetEntity( 0, this );
		}

		if ( !string.IsNullOrEmpty( FollowEffect ) )
			Follower = Particles.Create( FollowEffect, this );

		if ( !string.IsNullOrEmpty( ModelPath ) )
		{
			ModelEntity = SceneObject.CreateModel( ModelPath );
			ModelEntity.SetMaterialOverride( Material.Load( $"materials/{Team.GetTag()}_surface.vmat" ) );
		}
	}

	public virtual void Simulate()
	{
		var newPosition = Position;
		newPosition += Velocity * Time.Delta;

		GravityModifier += Gravity;
		newPosition -= new Vector3( 0f, 0f, GravityModifier * Time.Delta );

		var trace = Trace.Ray( Position, newPosition )
			.UseHitboxes()
			.Size( Radius )
			.WithoutTags( Team.GetTag(), "projectile" )
			.Run();

		Position = trace.EndPos;

		if ( DestroyTime )
		{
			Delete();
			return;
		}

		if ( HasHitTarget( ref trace ) )
		{
			if ( IsServer && !string.IsNullOrEmpty( HitSound ) )
			{
				CreateDecal( $"decals/{Team.GetTag()}.decal", ref trace );
				Audio.Play( HitSound, Position );

				OnHit( ref trace );
			}

			Delete();
		}
	}

	public bool HasClientProxy()
	{
		return !IsClientOnly && Owner.IsValid() && Owner.IsLocalPawn;
	}

	protected virtual bool HasHitTarget( ref TraceResult trace )
	{
		return (trace.Hit && CanHitTime) || trace.StartedSolid;
	}

	protected void CreateDecal( string decalname, ref TraceResult trace )
	{
		var decalPath = decalname;
		if ( decalPath != null )
		{
			if ( DecalDefinition.ByPath.TryGetValue( decalPath, out var decal ) )
				decal.PlaceUsingTrace( trace );
		}
	}

	protected virtual void OnHit( ref TraceResult trace )
	{
		if ( !trace.Entity.IsValid() )
			return;

		var info = new DamageInfo()
			.WithAttacker( Owner )
			.WithWeapon( Origin )
			.WithPosition( StartPosition )
			.WithForce( Velocity * 0.1f )
			.UsingTraceResult( trace );

		info.Damage = float.MaxValue;

		trace.Entity.TakeDamage( info );
	}


	[Event.Tick.Client]
	protected virtual void ClientTick()
	{
		if ( ModelEntity.IsValid() )
			ModelEntity.Transform = Transform;
	}

	[Event.Tick.Server]
	protected virtual void ServerTick()
	{
		if ( !Simulator.IsValid() )
			Simulate();
	}

	protected override void OnDestroy()
	{
		Simulator?.Remove( this );

		RemoveEffects();

		base.OnDestroy();
	}

	private void RemoveEffects()
	{
		ModelEntity?.Delete();
		Follower?.Destroy( true );
		Trail?.Destroy();
		Trail = null;
	}
}
