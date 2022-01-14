using Sandbox;
using System.Linq;

namespace PaintBall
{
	public class Inventory : BaseInventory
	{
		public Inventory( Player player ) : base( player ) { }

		public new Player Owner
		{
			get => base.Owner as Player;
			init => base.Owner = value;
		}

		public override bool Add( Entity entity, bool makeActive = false )
		{
			if ( entity is not Weapon weapon )
				return false;

			if ( weapon.ExclusiveFor != Team.None && weapon.ExclusiveFor != Owner.Team )
				return false;

			if ( List.Any( x => (x as Weapon).Slot == weapon.Slot ) )
				return false;

			return base.Add( entity, makeActive );
		}

		public override Entity DropActive()
		{
			var ac = Owner.ActiveChild as Weapon;

			if ( !ac.IsValid() || !ac.Droppable )
				return null;

			return base.DropActive();
		}

		public Bomb DropBomb()
		{
			if ( Owner.Team != Team.Red )
				return null;

			var bomb = List.Find( x => x is Bomb ) as Bomb;
			Drop( bomb );

			return bomb;
		}
	}
}
