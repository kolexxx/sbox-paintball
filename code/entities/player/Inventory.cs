using Sandbox;
using System.Linq;

namespace Paintball;

public class Inventory : BaseInventory
{
	public readonly int[] SlotCapacity = new int[] { 1, 1, 1, 3, 3 };

	public Inventory( Player player ) : base( player ) { }

	public new Player Owner
	{
		get => base.Owner as Player;
		init => base.Owner = value;
	}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		entity.Position = Owner.EyePos;

		if ( entity is not Weapon weapon )
			return false;

		if ( weapon.Config.ExclusiveFor != Team.None && weapon.Config.ExclusiveFor != Owner.Team )
			return false;

		if ( !HasFreeSlot( weapon.Config.Slot ) )
			return false;

		return base.Add( entity, makeActive );
	}

	public override void Pickup( Entity entity )
	{
		if ( entity is not Weapon weapon )
			return;

		if ( weapon.Config.ExclusiveFor != Team.None && weapon.Config.ExclusiveFor != Owner.Team )
			return;

		if ( !HasFreeSlot(weapon.Config.Slot ) )
			return;

		if ( base.Add( entity, Active == null ) )
			Audio.Play( "pickup_weapon", Owner.Position );
	}

	public override Entity DropActive()
	{
		var ac = Owner.ActiveChild as Weapon;

		if ( !ac.IsValid() || !ac.Droppable )
			return null;

		return base.DropActive();
	}

	public Weapon Swap( Weapon weapon )
	{
		var ent = List.Find( x => (x as Weapon).Config.Slot == weapon.Config.Slot );
		bool wasActive = ent?.IsActiveChild() ?? false;

		Drop( ent );
		Add( weapon, wasActive );

		return ent as Weapon;
	}

	public bool HasFreeSlot(SlotType slot )
	{
		int count = List.Where( x => (x as Weapon).Config.Slot == slot ).Count();

		return count < SlotCapacity[(int)slot];
	}

	public Bomb DropBomb()
	{
		if ( Owner.Team != Team.Red )
			return null;

		var bomb = List.Find( x => x is Bomb ) as Bomb;
		Drop( bomb );

		return bomb;
	}

	public override void DeleteContents()
	{
		Host.AssertServer();

		foreach ( var item in List.ToArray() )
		{
			item.OnCarryDrop( Owner );
			item.Delete();
		}

		List.Clear();
	}
}
