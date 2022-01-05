using Sandbox;
using System;

namespace PaintBall
{
	[Library]
	public partial class Projectile : ModelEntity
	{
		[Net, Predicted] public string FollowEffect { get; set; } = "";
		[Net, Predicted] public string HitSound { get; set; } = "";
		[Net, Predicted] public string Model { get; set; } = "";
		[Net, Predicted] public string TrailEffect { get; set; } = "";
		[Net] public bool IsServerOnly { get; set; } = false;
		public string Attachment { get; set; } = null;
		public Action<Projectile, Entity, int> Callback { get; private set; }
		public RealTimeUntil CanHitTime { get; set; } = 0.1f;
		public virtual bool ExplodeOnDestroy => false;
		public float Gravity { get; set; } = 0f;
		public string IgnoreTag { get; set; }
		public virtual float LifeTime => 10f;
		public float Radius { get; set; } = 4f;
		public ProjectileSimulator Simulator { get; set; }
		public Vector3 StartPosition { get; private set; }
		public Team Team { get; set; }
		protected RealTimeUntil DestroyTime { get; set; }
		protected Particles Follower { get; set; }
		protected float GravityModifier { get; set; }
		protected SceneObject ModelEntity { get; set; }
		protected Particles Trail { get; set; }

		public void Initialize( Vector3 start, Vector3 velocity, float radius, Action<Projectile, Entity, int> callback = null )
		{
			Initialize( start, velocity, callback );
			Radius = radius;
		}

		public void Initialize( Vector3 start, Vector3 velocity, Action<Projectile, Entity, int> callback = null )
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
			Callback = callback;
			Position = start;

			if ( IsClientOnly )
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
			if ( IsServerOnly || !HasClientProxy() )
				CreateEffects();

			base.ClientSpawn();
		}

		public virtual void CreateEffects()
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

			if ( !string.IsNullOrEmpty( Model ) )
				ModelEntity = SceneObject.CreateModel( Model );
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
				.WithoutTags( "baseprojectile", IgnoreTag, "grenade" )
				.Run();

			Position = trace.EndPos;

			if ( DestroyTime )
			{
				if ( ExplodeOnDestroy )
					OnExplode();

				Delete();
				return;
			}

			if ( HasHitTarget( trace ) )
			{
				if ( IsServer && !string.IsNullOrEmpty( HitSound ) )
				{
					CreateDecal( $"decals/{Team.GetString()}.decal", trace );
					Audio.Play( HitSound, Position );
				}

				Callback?.Invoke( this, trace.Entity, trace.Bone );
				Delete();
			}
		}

		public bool HasClientProxy()
		{
			return !IsClientOnly && Owner.IsValid() && Owner.IsLocalPawn;
		}

		protected virtual bool HasHitTarget( TraceResult trace )
		{
			return (trace.Hit && CanHitTime) || trace.StartedSolid;
		}

		protected void CreateDecal( string decalname, TraceResult tr )
		{
			var decalPath = decalname;
			if ( decalPath != null )
			{
				if ( DecalDefinition.ByPath.TryGetValue( decalPath, out var decal ) )
					decal.PlaceUsingTrace( tr );
			}
		}

		protected virtual void OnExplode() { }

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
}
