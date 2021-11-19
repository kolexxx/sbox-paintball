using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace PaintBall
{
	public class InventoryBar : Panel
	{
		private readonly List<InventoryIcon> slots = new();

		public InventoryBar()
		{
			for ( int i = 0; i < 6; i++ )
			{
				var icon = new InventoryIcon( i + 1, this );
				slots.Add( icon );
			}

			SetClass( "none", true );
		}

		public override void Tick()
		{
			base.Tick();

			var player = Local.Pawn;
			if ( player == null ) return;
			if ( player.Inventory == null ) return;

			for ( int i = 0; i < slots.Count; i++ )
			{
				UpdateIcon( player.Inventory.GetSlot( i ), slots[i], i );
			}
		}

		private static void UpdateIcon( Entity ent, InventoryIcon inventoryIcon, int i )
		{
			if ( ent == null )
			{
				inventoryIcon.Clear();
				return;
			}

			inventoryIcon.TargetEnt = ent;
			inventoryIcon.Label.Text = ent.ClassInfo.Title;
			inventoryIcon.SetClass( "active", ent.IsActiveChild() );
		}

		[Event.BuildInput]
		private void ProcessClientInput( InputBuilder input )
		{
			var player = Local.Pawn as Player;
			if ( player == null )
				return;

			var inventory = player.Inventory;
			if ( inventory == null )
				return;
			
			if ( input.Pressed( InputButton.Slot1 ) ) SetActiveSlot( input, inventory, 0 );
			if ( input.Pressed( InputButton.Slot2 ) ) SetActiveSlot( input, inventory, 1 );
			if ( input.Pressed( InputButton.Slot3 ) ) SetActiveSlot( input, inventory, 2 );
			if ( input.Pressed( InputButton.Slot4 ) ) SetActiveSlot( input, inventory, 3 );
			if ( input.Pressed( InputButton.Slot5 ) ) SetActiveSlot( input, inventory, 4 );
		}

		private static void SetActiveSlot( InputBuilder input, IBaseInventory inventory, int i )
		{
			var player = Local.Pawn;
			if ( player == null )
				return;

			var ent = inventory.GetSlot( i );
			if ( player.ActiveChild == ent )
				return;

			if ( ent == null )
				return;

			input.ActiveChild = ent;
		}
	}

	public class InventoryIcon : Panel
	{
		public Entity TargetEnt;
		public Label Label;
		public Label Number;

		public InventoryIcon( int i, Panel parent )
		{
			Parent = parent;
			Label = Add.Label( "empty", "item-name" );
			Number = Add.Label( $"{i}", "slot-number" );
		}

		public void Clear()
		{
			Label.Text = "";
			SetClass( "active", false );
		}
	}
}
