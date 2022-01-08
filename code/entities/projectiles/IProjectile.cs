using Sandbox;

namespace PaintBall
{
	public interface IProjectile
	{
		public RealTimeUntil DestroyTime { get; }
		public float LifeTime { get; }
		public string ModelPath { get; }
		public Entity Origin { get; }
		public Team Team { get; }		
	}
}
