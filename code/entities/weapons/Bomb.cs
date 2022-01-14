using Sandbox;

namespace PaintBall
{
	[Library( "pb_bomb", Title = "Bomb", Description = "A bomb that can be planted on a bombsite.", Spawnable = false )]
	[Hammer.EditorModel( "weapons/rust_shotgun/rust_shotgun.vmdl" )]
	public partial class Bomb : Weapon
	{
		[Net] public TimeUntil TimeUntilPlant { get; set; }
		public override bool Automatic => true;
		public override SlotType Slot => SlotType.Deployable;
		public override int ClipSize => 1;
		public override Team ExclusiveFor => Team.Red;
		public override string FireSound => "";
		public override string Icon => "ui/weapons/bomb.png";
		public override float PrimaryRate => 0;
		public override float ReloadTime => 2.0f;
		public override string ViewModelPath => "weapons/rust_shotgun/v_rust_shotgun.vmdl";

		public override void ActiveStart( Entity entity )
		{
			base.ActiveStart( entity );

			TimeUntilPlant = 5f;
		}

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = ClipSize;
			SetModel( "weapons/rust_shotgun/rust_shotgun.vmdl" );
		}

		public override bool CanPrimaryAttack()
		{
			if ( Owner.CanPlantBomb && base.CanPrimaryAttack() )
				return true;

			TimeUntilPlant = 1f;
			return false;
		}

		public override void AttackPrimary()
		{
			base.AttackPrimary();

			if ( TimeUntilPlant )
			{
				FinishPlanting();

				var trace = Trace.Ray( Owner.Position, Owner.Position + Vector3.Down * 64 )
					.HitLayer( CollisionLayer.WORLD_GEOMETRY )
					.Run();

				if ( IsServer )
				{
					using ( Prediction.Off() )
					{
						var bomb = new PlantedBomb();
						bomb.Position = trace.EndPos;
						bomb.Rotation = Owner.Rotation;
						bomb.Planter = Owner;
					}

					Owner.SwitchToBestWeapon();
					Delete();
				}
			}
		}

		[ClientRpc]
		protected void FinishPlanting()
		{
			// finish animation
		}
	}
}
