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

		private InventoryIcon[] Slots = new InventoryIcon[5];
		private Weapon[] Weapons = new Weapon[5];
		RealTimeUntil Close;

		public InventoryBar()
		{
			Instance = this;

			StyleSheet.Load( "/ui/InventoryBar.scss" );

			for ( int i = 0; i < 5; i++ )
				Slots[i] = new InventoryIcon( i + 1, this );

		}

		public override void Tick()
		{
			base.Tick();

			if ( Local.Pawn is not Player player )
				return;

			for ( int i = 0; i < 5; i++ )
				Weapons[i] = null;

			foreach ( var weapon in player.CurrentPlayer.Children.OfType<Weapon>() )
				Weapons[weapon.Bucket] = weapon;

			for ( int i = 0; i < 5; i++ )
			{
				if ( Weapons[i] == null )
				{
					if ( Slots[i].TargetWeapon != null )
						Close = 3f;

					Slots[i].Clear();
					continue;
				}

				if ( Slots[i].TargetWeapon == null || Slots[i].TargetWeapon != Weapons[i] )
					Close = 3f;

				Slots[i].UpdateWeapon( Weapons[i] );
			}

			SetClass( "hidden", Close <= 0 );
		}

		[Event.BuildInput]
		private void BuildInput( InputBuilder input )
		{
			if ( Local.Pawn is not Player player )
				return;
			/*
			if ( player.TimeSinceSpawned <= 0.1f )
			{
				Fix( input );

				return;
			}*/

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
			Close = 3f;

			var player = Local.Pawn as Player;

			if ( player == null || player.CurrentPlayer != player )
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
			for ( int i = 4; i >= 0; i-- )
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

				if ( weapon.IsActiveChild() && !HasClass( "active" ) )
					Instance.Close = 3f;

				SetClass( "active", weapon.IsActiveChild() );
				SetClass( "hidden", false );
			}
		}
	}
}
