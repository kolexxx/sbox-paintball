using Sandbox;

namespace PaintBall;

public static class Debug
{
	public static void CheckRealms()
	{
		Log.Info( Host.IsServer ? "Server: " : "Client: " );
	}
}
