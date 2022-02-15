using Sandbox;

namespace Paintball;

[Hammer.AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
[Hammer.Solid]
[Hammer.VisGroup( Hammer.VisGroup.Trigger )]
[Library( "pb_buyzone", Description = "Place where players can buy items." )]
public partial class BuyZone : BaseTrigger
{
	[Net, Property] public Team Team { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SurroundingBoundsMode = SurroundingBoundsType.Obb;
		Transmit = TransmitType.Always;
		Parent = Game.Current;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		EnableTouch = true;
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	public override void OnTouchStart( Entity toucher )
	{
		base.OnTouchStart( toucher );

		(toucher as Player).IsInBuyZone = true;
	}

	public override void OnTouchEnd( Entity toucher )
	{
		base.EndTouch( toucher );

		(toucher as Player).IsInBuyZone = false;
	}

	public override bool PassesTriggerFilters( Entity other )
	{
		return other is Player player && player.Alive() && (Team == Team.None || player.Team == Team);
	}
}

