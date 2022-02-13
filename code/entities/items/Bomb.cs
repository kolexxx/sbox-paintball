﻿using Sandbox;

namespace Paintball;

[Library( "pb_bomb", Title = "Bomb", Description = "A bomb that can be planted on a bombsite.", Spawnable = false )]
[Hammer.EditorModel( "weapons/rust_shotgun/rust_shotgun.vmdl" )]
[Hammer.Skip]
public sealed partial class Bomb : Carriable
{
	[Net, Predicted] public TimeSince Delay { get; set; } = 2f;
	[Net] public TimeSince TimeSinceStartedPlanting { get; set; } = 0f;

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

	public override void Simulate( Client owner )
	{
		if ( TimeSinceDeployed < Info.DeployTime )
			return;

		if ( CanPlant() )
			Plant();
	}

	public bool CanPlant()
	{
		if ( Input.Down( InputButton.Attack1 ) && Owner.CanPlantBomb && Delay >= 2f )
		{
			if ( !Owner.IsPlantingBomb )
			{
				PlaySound( "started_planting" );
				PlantEffects();
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

	public void Plant()
	{
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

		if ( !dropper.Alive() )
			UI.Notification.Create( Team.Red.ToClients(), "Bomb has been dropped!", 2f );
	}

	[ClientRpc]
	private void PlantEffects()
	{

	}
}
