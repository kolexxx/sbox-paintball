using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace PaintBall
{
	public class InventoryBar : Panel
	{
		private List<InventoryIcon> Slots = new();
		private List<Weapon> Weapons = new();

		public InventoryBar()
		{
			StyleSheet.Load( "/ui/InventoryBar.scss" );

			for ( int i = 0; i < 5; i++ )
			{
				var icon = new InventoryIcon( i + 1, this );
				Slots.Add( icon );
			}
		}

		public override void Tick()
		{
			base.Tick();

			var player = Local.Pawn;
			if ( player == null ) return;

			SetClass( "hidden", player.LifeState != LifeState.Alive );

			if ( !IsVisible )
				return;

			Weapons = player.Children.OfType<Weapon>().ToList();
			Weapons.Sort( ( a, b ) => a.Bucket.CompareTo( b.Bucket ) );

			int i = 0;

			for ( int index = i; index < Weapons.Count; index++, i++ )
				Slots[Weapons[index].Bucket].UpdateWeapon( Weapons[index] );

			for ( int index = i; index < Slots.Count; index++ )
				Slots[index].Clear();

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

			if ( input.Pressed( InputButton.Slot1 ) ) SetActiveSlot( input, 0 );
			if ( input.Pressed( InputButton.Slot2 ) ) SetActiveSlot( input, 1 );
			if ( input.Pressed( InputButton.Slot3 ) ) SetActiveSlot( input, 2 );
			if ( input.Pressed( InputButton.Slot4 ) ) SetActiveSlot( input, 3 );
			if ( input.Pressed( InputButton.Slot5 ) ) SetActiveSlot( input, 4 );
		}

		private void SetActiveSlot( InputBuilder input, int i )
		{
			if ( i >= Weapons.Count )
				return;

			var player = Local.Pawn;
			if ( player == null )
				return;

			var weapon = Weapons[i];

			if ( player.ActiveChild == weapon )
				return;

			if ( weapon == null )
				return;

			input.ActiveChild = weapon;
		}

		public class InventoryIcon : Panel
		{
			public Weapon TargetWeapon;
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
				TargetWeapon = null;
				Label.Text = "";
				SetClass( "hidden", true );
			}

			public void UpdateWeapon( Weapon weapon )
			{
				TargetWeapon = weapon;
				Label.Text = weapon.Name;
				SetClass( "hidden", false );
			}
		}
	}
}
