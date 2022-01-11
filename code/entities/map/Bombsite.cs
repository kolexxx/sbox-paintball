using Sandbox;

namespace PaintBall
{
	[Library( "pb_bombsite", Description = "Place where the Red Team can plant the bomb." )]
	[Hammer.AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
	[Hammer.Solid]
	[Hammer.VisGroup( Hammer.VisGroup.Trigger )]
	public class Bombsite : BaseTrigger
	{
	}
}
