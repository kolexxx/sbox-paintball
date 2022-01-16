using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

namespace PaintBall;

public class InventoryBar : Panel
{
	public static InventoryBar Instance;
	private InventoryIcon[] _slots = new InventoryIcon[5];
	private Weapon[] _weapons = new Weapon[5];
	private RealTimeUntil _close;

	public InventoryBar()
	{
		Instance = this;

		StyleSheet.Load( "/ui/InventoryBar.scss" );

		for ( int i = 0; i < 5; i++ )
			_slots[i] = new InventoryIcon( i + 1, this );

		BindClass( "hidden", () => TeamSelect.Instance.IsVisible );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Player player )
			return;

		for ( int i = 0; i < 5; i++ )
			_weapons[i] = null;

		foreach ( var weapon in player.CurrentPlayer.Children.OfType<Weapon>() )
			_weapons[(int)weapon.Slot] = weapon;

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
	private void BuildInput(InputBuilder input)
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

	private void SetActiveSlot(InputBuilder input, int i)
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
		public Weapon TargetWeapon;
		public Image Icon;
		public Label Number;

		public InventoryIcon(int i, Panel parent)
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

		public void UpdateWeapon(Weapon weapon)
		{
			TargetWeapon = weapon;
			Icon.SetTexture( weapon?.Icon );

			if ( weapon.IsActiveChild() && !HasClass( "active" ) )
				Instance._close = 3f;

			SetClass( "active", weapon.IsActiveChild() );
			SetClass( "hidden", false );
		}
	}
}
