using Sandbox;
using System;
using System.Linq;

namespace PaintBall
{
	public class Inventory : BaseInventory
	{
		public Inventory( Player player ) : base( player )
		{

		}

		public override bool Add( Entity entity, bool makeActive = false )
		{
			if ( entity is not Weapon weapon)
				return false;

			if ( List.Any( x => (x as Weapon).Bucket == weapon.Bucket ) )		
				return false;
			
			return base.Add( entity, makeActive );
		}
	}
}
