using Sandbox;

namespace Paintball;

public partial class Player
{
	[Net] public int Money { get; set; } = 1000;

	[ServerCmd]
	public static void RequestItem( string libraryName )
	{
		if ( Game.Current.State is not GameplayState state )
			return;

		if ( state.RoundState != RoundState.Freeze || string.IsNullOrEmpty( libraryName ) )
			return;

		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() )
			return;

		var config = ItemConfig.All[libraryName];

		if ( player.Money < config.Price )
			return;

		player.Money -= config.Price;
		player.Inventory.Add( Library.Create<Entity>( libraryName ) );
	}
}

