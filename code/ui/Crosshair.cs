using Sandbox;
using Sandbox.UI;
using System;

namespace PaintBall
{
	public class Crosshair : Panel
	{
		public Weapon TargetWeapon { get; set; }
		private int _fireCount = 0;

		public Crosshair()
		{
			StyleSheet.Load( "/ui/Crosshair.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is not Player player )
				return;

			SetClass( "hidden", Local.Hud.GetChild( 7 ).IsVisible || Local.Hud.GetChild( 10 ).IsVisible || !TargetWeapon.ViewModelEntity.EnableDrawing );	
			SetClass( "fire", _fireCount > 0 );
			
			_fireCount = Math.Max( 0, _fireCount - 1 );
		}

		[PanelEvent]
		public void FireEvent() { _fireCount += 5; }
	}
}
