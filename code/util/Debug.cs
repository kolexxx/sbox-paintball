using Sandbox;

namespace Paintball;

public static class Debug
{
	public static string CheckRealms()
	{
		return Host.IsServer ? "Server: " : "Client: " ;
	}
}
