using Sandbox;

namespace PaintBall;

[Library( "pb_bombsite", Description = "Place where the Red Team can plant the bomb." )]
[Hammer.AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
[Hammer.Solid]
[Hammer.VisGroup( Hammer.VisGroup.Trigger )]
public partial class Bombsite : BaseTrigger
{
	[Net, Property] public char Letter { get; set; }
	protected Output BombPlanted { get; set; }
	protected Output BombExplode { get; set; }
	protected Output BombDefused { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SurroundingBoundsMode = SurroundingBoundsType.Obb;
		Transmit = TransmitType.Always;
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

		(toucher as Player).Bombsite = this;
	}

	public override void OnTouchEnd( Entity toucher )
	{
		base.EndTouch( toucher );

		(toucher as Player).Bombsite = null;
	}

	public override bool PassesTriggerFilters( Entity other )
	{
		return other is Player player && player.Alive();
	}
}
