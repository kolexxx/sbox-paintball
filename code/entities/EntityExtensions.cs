using Sandbox;

namespace PaintBall;

public static class EntityExtensions
{
	public static bool Alive(this Entity entity) => entity.LifeState == LifeState.Alive;
}
