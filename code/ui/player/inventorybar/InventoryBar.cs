using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace Paintball.UI;

public class InventoryBar : Panel
{
	public static InventoryBar Instance;
	private InventoryIcon[] _slots = new InventoryIcon[5];
	private Carriable[] _weapons = new Carriable[5];
	private RealTimeUntil _close;

	public InventoryBar()
	{
		Instance = this;

		StyleSheet.Load( "/ui/player/inventorybar/InventoryBar.scss" );

		for ( int i = 0; i < 5; i++ )
			_slots[i] = new InventoryIcon( i + 1, this );

		BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player player )
			return;

		if ( !player.Alive() && !player.IsSpectatingPlayer )
			return;

		for ( int i = 0; i < 5; i++ )
			_weapons[i] = null;

		foreach ( var weapon in player.CurrentPlayer.Children.OfType<Carriable>() )
			_weapons[(int)weapon.Info.Slot] = weapon;

		for ( int i = 0; i < 5; i++ )
		{
			if ( _weapons[i] == null )
			{
				if ( _slots[i].TargetWeapon != null )
					_close = 3f;

				_slots[i].Clear();
				continue;
			}

			if ( _slots[i].TargetWeapon == null || _slots[i].TargetWeapon != _weapons[i] )
				_close = 3f;

			_slots[i].UpdateWeapon( _weapons[i] );
		}

		SetClass( "hidden", _close <= 0 );
	}

	[Event.BuildInput]
	private void BuildInput( InputBuilder input )
	{
		if ( Local.Pawn is not Player player )
			return;

		if ( player.CurrentPlayer != player )
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
		_close = 3f;

		var player = Local.Pawn as Player;

		if ( player == null || player.CurrentPlayer != player )
			return;

		var weapon = _weapons[i];

		if ( player.ActiveChild == weapon )
			return;

		if ( weapon == null )
			return;

		input.ActiveChild = weapon;
	}

	public class InventoryIcon : Panel
	{
		public Carriable TargetWeapon;
		public Image Icon;
		public InputHint InputHint;

		public InventoryIcon( int i, Panel parent )
		{
			Parent = parent;
			Icon = Add.Image( string.Empty, "slot-icon" );

			InputHint = AddChild<InputHint>();

			InputButton button = 0;

			switch ( i )
			{
				case 1:
					button = InputButton.Slot1;
					break;
				case 2:
					button = InputButton.Slot2;
					break;
				case 3:
					button = InputButton.Slot3;
					break;
				case 4:
					button = InputButton.Slot4;
					break;
				case 5:
					button = InputButton.Slot5;
					break;
			}

			InputHint.SetButton( button );
		}

		public void Clear()
		{
			TargetWeapon = null;
			SetClass( "hidden", true );
		}

		public void UpdateWeapon( Carriable weapon )
		{
			TargetWeapon = weapon;
			Icon.SetTexture( weapon?.Info.Icon );

			if ( weapon.IsActiveChild() && !HasClass( "active" ) )
				Instance._close = 3f;

			InputHint.SetClass( "hidden", !Local.Pawn.Alive() );
			SetClass( "active", weapon.IsActiveChild() );
			SetClass( "hidden", false );
		}
	}
}
