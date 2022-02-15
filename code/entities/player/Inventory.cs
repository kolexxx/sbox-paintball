using Sandbox;

namespace Paintball;

public class Inventory : BaseInventory
{
	public int[] SlotCapacity = new int[] { 1, 1, 1, 3, 1 };

	public Inventory( Player player ) : base( player ) { }

	public new Player Owner
	{
		get => base.Owner as Player;
		init => base.Owner = value;
	}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		entity.Position = Owner.EyePosition;

		return base.Add( entity, makeActive );
	}

	public override void Pickup( Entity entity )
	{
		if ( base.Add( entity, Active == null ) )
			Audio.Play( "pickup_weapon", Owner.Position );
	}

	public override Entity DropActive()
	{
		var ac = Owner.ActiveChild as Carriable;

		if ( !ac.IsValid() || !ac.Droppable )
			return null;

		return base.DropActive();
	}

	public Carriable Swap( Carriable carriable )
	{
		var ent = List.Find( x => (x as Carriable).Info.Slot == carriable.Info.Slot );
		bool wasActive = ent?.IsActiveChild() ?? false;

		Drop( ent );
		Add( carriable, wasActive );

		return ent as Carriable;
	}

	public bool HasFreeSlot( SlotType slot )
	{
		return SlotCapacity[(int)slot] > 0;
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
