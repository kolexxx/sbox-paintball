using Sandbox;

namespace Paintball;

[Library( "pb_spectatepoint" )]
[Hammer.EditorModel( "models/editor/camera.vmdl" )]
[Hammer.EntityTool( "Spectate Point", "PaintBall", "Defines a fixed point where a player can spectate from." )]
public class SpectatePoint : Entity
{
	[Property] public int Index { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
