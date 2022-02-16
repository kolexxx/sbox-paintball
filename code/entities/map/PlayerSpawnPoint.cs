using Sandbox;

namespace Paintball;

[Hammer.EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
[Hammer.EntityTool( "Player Spawnpoint", "Paintball", "Defines a point where players on a team can spawn." )]
[Library( "pb_spawnpoint" )]
public class PlayerSpawnPoint : Entity
{
	[Property] public Team Team { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Parent = Game.Current;
	}
}
