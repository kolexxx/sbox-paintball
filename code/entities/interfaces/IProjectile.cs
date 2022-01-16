using Sandbox;

namespace PaintBall;

public interface IProjectile : ITeamEntity
{
	public RealTimeUntil DestroyTime { get; }
	public float LifeTime { get; }
	public string ModelPath { get; }
	public Entity Origin { get; }
}
