﻿using Sandbox;

namespace Paintball;

[Config( SlotType.Deployable, Team.Red )]
[Library( "pb_bomb", Title = "Bomb", Description = "A bomb that can be planted on a bombsite.", Icon = "ui/weapons/bomb.png", Spawnable = false )]
[Hammer.EditorModel( "weapons/rust_shotgun/rust_shotgun.vmdl" )]
[Hammer.Skip]
public sealed partial class Bomb : Weapon
{
	[Net, Predicted] public TimeSince Delay { get; set; } = 2f;
	[Net] public TimeSince TimeSinceStartedPlanting { get; set; } = 0f;
	public override bool Automatic => true;
	public override int ClipSize => 1;
	public override string FireSound => "";
	public override float PrimaryRate => 0;
	public override float ReloadTime => 2.0f;
	public override string ViewModelPath => "weapons/rust_shotgun/v_rust_shotgun.vmdl";

	public override void ActiveStart( Entity entity )
	{
		base.ActiveStart( entity );

		TimeSinceStartedPlanting = 0;
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		if ( Owner.IsValid() )
			Owner.IsPlantingBomb = false;
	}

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = ClipSize;
		SetModel( "weapons/rust_shotgun/rust_shotgun.vmdl" );
	}

	public override bool CanPrimaryAttack()
	{
		if ( base.CanPrimaryAttack() && Owner.CanPlantBomb && Delay >= 2f )
		{
			if ( !Owner.IsPlantingBomb )
			{
				PlaySound( "started_planting" );
				ShootEffects();
			}

			Owner.IsPlantingBomb = true;
			return true;
		}

		if ( Owner.IsPlantingBomb )
			Delay = 0;

		TimeSinceStartedPlanting = 0f;
		Owner.IsPlantingBomb = false;
		return false;
	}

	public override void AttackPrimary()
	{
		base.AttackPrimary();

		if ( TimeSinceStartedPlanting >= 2f )
		{
			Owner.IsPlantingBomb = false;

			var trace = Trace.Ray( Owner.Position, Owner.Position + Vector3.Down * 128 )
				.WorldOnly()
				.Run();

			Owner.SwitchToBestWeapon();

			if ( IsServer )
			{
				var bomb = new PlantedBomb
				{
					Position = trace.EndPos,
					Rotation = Owner.Rotation,
					Planter = Owner,
					Bombsite = Owner.Bombsite
				};

				bomb.Initialize();

				Delete();
			}
		}
	}

	public override void OnCarryDrop( Entity dropper )
	{
		base.OnCarryDrop( dropper );

		if ( dropper is not Player player )
			return;

		player.IsPlantingBomb = false;
	}
}
