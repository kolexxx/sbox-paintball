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
			return base.Add( entity, makeActive );
		}

		public bool IsCarryingType( Type t )
		{
			return List.Any( x => x.GetType() == t );
		}
	}
}
