using Sandbox;

namespace Paintball;

public static class Debug
{
	public static void CheckRealms()
	{
		Log.Info( Host.IsServer ? "Server: " : "Client: " );
	}
}
