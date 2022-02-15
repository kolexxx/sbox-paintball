using Sandbox;

namespace Paintball;

[Hammer.EditorModel( "models/editor/camera.vmdl" )]
[Hammer.EntityTool( "Spectate Point", "Paintball", "Defines a fixed point where a player can spectate from." )]
[Library( "pb_spectatepoint" )]
public class SpectatePoint : Entity
{
	[Property] public int Index { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		Parent = Game.Current;
	}
}
