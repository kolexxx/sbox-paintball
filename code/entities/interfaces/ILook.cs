using Sandbox;

namespace PaintBall;

public interface ILook
{
	public bool IsLookable( Entity viewer );
	public void StartLook( Entity viewer );
	public void EndLook( Entity viewer );
}
