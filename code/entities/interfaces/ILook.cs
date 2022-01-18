using Sandbox;

namespace PaintBall;

public interface ILook
{
	public bool IsLookable( Entity viewer );
	public void StartLook();
	public void Update();
	public void EndLook();
}
