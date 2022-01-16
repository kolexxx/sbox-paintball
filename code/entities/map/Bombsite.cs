using Sandbox;

namespace PaintBall;

[Library( "pb_bombsite", Description = "Place where the Red Team can plant the bomb." )]
[Hammer.AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
[Hammer.Solid]
[Hammer.VisGroup( Hammer.VisGroup.Trigger )]
public partial class Bombsite : BaseTrigger
{
	[Net, Property] public char Letter { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SurroundingBoundsMode = SurroundingBoundsType.Obb;
		Transmit = TransmitType.Always;
	}

	public override void OnTouchStart( Entity toucher )
	{
		base.OnTouchStart( toucher );

		(toucher as Player).IsOnBombsite = true;
	}

	public override void OnTouchEnd( Entity toucher )
	{
		base.EndTouch( toucher );

		(toucher as Player).IsOnBombsite = false;
	}

	public override bool PassesTriggerFilters( Entity other )
	{
		return other is Player player && player.Alive();
	}
}
