using Sandbox;

namespace PaintBall
{
	[Library( "pb_grenade", Title = "grenade", Spawnable = true )]
	[Hammer.EditorModel( "models/grenade/grenade.vmdl" )]
	public sealed partial class Throwable : Weapon
	{
		public override int Bucket => 3;
		public override int ClipSize => 1;
		public override string FireSound => "";
		public override string Icon => "ui/weapons/grenade.png";
		public override float PrimaryRate => 15f;
		public override float ReloadTime => 2.0f;
		public override string ViewModelPath => "models/grenade/v_grenade.vmdl";

		public override void Spawn()
		{
			base.Spawn();

			AmmoClip = ClipSize;

			SetModel( "models/grenade/grenade.vmdl" );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", 5 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		public override void AttackPrimary()
		{
			base.AttackPrimary();

			TimeSincePrimaryAttack = 0;

			var trace = Trace.Ray( Owner.EyePos, (Owner.EyeRot.Forward * 40) ).Run();

			if ( !IsServer )
				return;

			using ( Prediction.Off() )
			{
				var ent = new PhysicsProjectile();
				ent.Rotation = Owner.EyeRot;
				ent.Position = trace.EndPos;
				ent.Velocity = Owner.EyeRot.Forward * 1000 + Vector3.Up * 100;
				ent.Owner = Owner;
				ent.Origin = this;

				(Owner as Player).SwitchToBestWeapon();
				Delete();
			}
		}
	}
}
