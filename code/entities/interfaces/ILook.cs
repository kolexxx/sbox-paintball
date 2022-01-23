using Sandbox;

namespace Paintball;

public interface ILook
{
	public bool IsLookable( Entity viewer );
	public void StartLook( Entity viewer );
	public void EndLook( Entity viewer );
}
