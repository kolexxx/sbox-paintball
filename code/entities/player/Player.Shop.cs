using Sandbox;
using System;

namespace Paintball;

public partial class Player
{
	[ServerVar( "pb_playermoneycap" )] public static int MoneyCap { get; set; } = 10000;
	[Net, Change] public int Money { get; set; } = 1000;
	public bool IsInBuyZone { get; set; } = true;

	public void AddMoney( int amount )
	{
		Host.AssertServer();

		Money += amount;
		Money = Math.Min( Money, MoneyCap );
	}

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

		player.AddMoney( -info.Price );
		player.Inventory.Swap( Library.Create<Carriable>( libraryName ) );
	}

	[PBEvent.Round.End]
	private void OnRoundEnd( Team winner )
	{
		if ( !IsServer )
			return;

		if ( winner == Team.None )
			return;

		if ( winner == Team )
			AddMoney( 2000 );
		else
			AddMoney( 1000 );
	}

	private void OnMoneyChanged( int oldMoney, int newMoney )
	{
		if ( this != (Local.Pawn as Player).CurrentPlayer )
			return;

		UI.Money.Instance.AnimateChange( newMoney - oldMoney );
	}
}

