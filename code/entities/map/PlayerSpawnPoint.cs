using Sandbox;

namespace PaintBall
{
	[Library( "pb_spawnpoint" )]
	[Hammer.EditorModel( "models/editor/playerstart.vmdl", FixedBounds = true )]
	[Hammer.EntityTool( "Player Spawnpoint", "PaintBall", "Defines a point where players on a team can spawn" )]
	public partial class PlayerSpawnPoint : Entity
	{
		[Property] public Team Team { get; set; }
		public bool Occupied { get; set; }
	}
}
