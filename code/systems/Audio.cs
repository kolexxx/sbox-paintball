using Sandbox;

namespace PaintBall
{
	public static partial class Audio
	{
		public static void PlayAll( string sound )
		{
			Play( To.Everyone, sound );
		}

		[ClientRpc]
		public static void Play( string sound )
		{
			Sound.FromScreen( sound );
		}

		[ClientRpc]
		public static void Play( string sound, Vector3 position )
		{
			Sound.FromWorld( sound, position );
		}
	}
}
