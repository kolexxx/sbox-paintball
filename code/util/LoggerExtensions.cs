using Sandbox;

namespace Paintball;

public static class LoggerExtensions
{
	public static void Debug( this Logger log, object obj = null )
	{
		string host = Host.IsServer ? "SERVER" : "CLIENT";

		log.Info( $"[DEBUG][{host}] {obj}" );
	}
}
