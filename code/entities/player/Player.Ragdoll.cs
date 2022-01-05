using Sandbox;

namespace PaintBall
{
	partial class Player
	{
		[ClientRpc]
		private void BecomeRagdollOnClient( Vector3 force, int forceBone )
		{
			var ent = new ModelEntity();
			ent.Position = Position;
			ent.Rotation = Rotation;
			ent.Scale = Scale;
			ent.MoveType = MoveType.Physics;
			ent.UsePhysicsCollision = true;
			ent.EnableAllCollisions = true;
			ent.CollisionGroup = CollisionGroup.Debris;
			ent.SetModel( GetModelName() );
			ent.CopyBonesFrom( this );
			ent.CopyBodyGroups( this );
			ent.CopyMaterialGroup( this );
			ent.TakeDecalsFrom( this );
			ent.EnableHitboxes = true;
			ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
			ent.RenderColor = Color.Gray;

			ent.SetInteractsAs( CollisionLayer.Debris );
			ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
			ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

			ent.SetRagdollVelocityFrom( this );

			ent.PhysicsGroup.AddVelocity( force );

			if ( forceBone >= 0 )
			{
				var body = ent.GetBonePhysicsBody( forceBone );

				if ( body != null )
					body.ApplyForce( force * 1000 );
				else
					ent.PhysicsGroup.AddVelocity( force );
			}

			Corpse = ent;

			ent.DeleteAsync( 10.0f );
		}

		[ClientRpc]
		public void RemoveCorpseOnClient()
		{
			RemoveCorpse();
		}

		public void RemoveCorpse()
		{
			if ( IsServer )
				RemoveCorpseOnClient();

			if ( Corpse != null && Corpse.IsValid() )
			{
				Corpse.Delete();
				Corpse = null;
			}
		}
	}
}
