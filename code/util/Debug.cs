using Sandbox;

namespace PaintBall
{
	public static class Debug
	{
		public static void CheckRealms()
		{
			Log.Info( $"Client : {Host.IsClient} | Server : {Host.IsServer}" );
		}
	}
}
