using Sandbox;
using System;

namespace Paintball;

public partial class Player
{
	[Net] public int Money { get; set; } = 1000;

	[ServerCmd]
	public static void RequestItem( string typeName )
	{
		if ( Game.Current.State is not GameplayState state )
			return;

		if ( state.RoundState != RoundState.Freeze || string.IsNullOrEmpty( typeName ) )
			return;

		Type type = Type.GetType( typeName );
		var ent = Library.Create<Entity>(typeName);
	}
}

