using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace PaintBall
{
	public class InventoryBar : Panel
	{
		public static InventoryBar Instance;

		private List<InventoryIcon> Slots = new();
		private Weapon[] Weapons = new Weapon[5];

		public InventoryBar()
		{
			Instance = this;

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

			if ( Local.Pawn is not Player player )
				return;

			for ( int i = 0; i < 5; i++ )
				Weapons[i] = null;

			foreach ( var weapon in player.CurrentPlayer.Children.OfType<Weapon>().ToList() )
				Weapons[weapon.Bucket] = weapon;

			for ( int i = 0; i < 5; i++ )
			{
				if ( Weapons[i] == null )
				{
					Slots[i].Clear();
					continue;
				}

				Slots[i].UpdateWeapon( Weapons[i] );
			}
		}

		[Event.BuildInput]
		private void ProcessClientInput( InputBuilder input )
		{
			var player = Local.Pawn as Player;
			if ( player == null )
				return;

			if ( player.FixSpawn <= 0.1f )
			{
				Fix( input );
				
				return;
			}

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

		public void Fix( InputBuilder input )
		{
			for ( int i = 0; i < 5; i++ )
			{
				SetActiveSlot( input, i );
			}
		}

		public class InventoryIcon : Panel
		{
			public Weapon TargetWeapon;
			public Image Icon;
			public Label Number;

			public InventoryIcon( int i, Panel parent )
			{
				Parent = parent;
				Icon = Add.Image( "", "slot-icon" );
				Number = Add.Label( $"{i}", "slot-number" );
			}

			public void Clear()
			{
				TargetWeapon = null;
				SetClass( "hidden", true );
			}

			public void UpdateWeapon( Weapon weapon )
			{
				TargetWeapon = weapon;
				Icon.SetTexture( weapon?.Icon );
				SetClass( "hidden", false );
			}
		}
	}
}
