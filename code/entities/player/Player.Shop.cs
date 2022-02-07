using Sandbox;

namespace Paintball;

public partial class Player
{
	[Net] public int Money { get; set; } = 1000;
	public bool IsInBuyZone { get; set; } = true;

	[ServerCmd]
	public static void RequestItem( string libraryName )
	{
		if ( string.IsNullOrEmpty( libraryName ) )
			return;

		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() || !(player.IsInBuyZone && Game.Current.State.CanBuy) )
			return;

		var config = ItemConfig.All[libraryName];

		if ( player.Money < config.Price )
			return;

		player.Money -= config.Price;
		player.Inventory.Add( Library.Create<Entity>( libraryName ) );
	}
}

