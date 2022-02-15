using Sandbox;

namespace Paintball;

public partial class Player
{
	[Net, Change] public int Money { get; set; } = 1000;
	public bool IsInBuyZone { get; set; }

	[ServerCmd]
	public static void RequestItem( string libraryName )
	{
		if ( string.IsNullOrEmpty( libraryName ) )
			return;

		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() || !(player.IsInBuyZone && Game.Current.State.CanBuy) )
			return;

		var info = CarriableInfo.All[libraryName];

		if ( !info.Buyable )
		{
			Log.Warning( "Tried to request an unbuyable item!" );
			return;
		}

		if ( player.Money < info.Price )
			return;

		player.Money -= info.Price;
		player.Inventory.Swap( Library.Create<Carriable>( libraryName ) );
	}

	private void OnMoneyChanged( int oldMoney, int newMoney )
	{
		if ( this != (Local.Pawn as Player).CurrentPlayer )
			return;

		UI.Money.Instance.AnimateChange( newMoney - oldMoney );
	}
}

