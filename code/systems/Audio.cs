using Sandbox;

namespace PaintBall
{
	public static partial class Audio
	{
		public enum Priority
		{
			None,
			Low,
			Medium,
			High
		}

		private static Sound s_currentAnnouncement;
		private static Priority s_currentPriority;

		public static void AnnounceAll( string sound, Priority priority )
		{
			Announce( To.Everyone, sound, priority );
		}

		[ClientRpc]
		public static void Announce( string sound, Priority priority )
		{
			if ( priority < s_currentPriority && s_currentAnnouncement.Finished )
			{
				s_currentAnnouncement = Sound.FromScreen( sound );
				s_currentPriority = priority;
			}
			else if ( priority >= s_currentPriority )
			{
				s_currentAnnouncement.Stop();
				s_currentAnnouncement = Sound.FromScreen( sound );
				s_currentPriority = priority;
			}
		}

		public static void PlayAll(string sound )
		{
			Play( To.Everyone, sound );
		}

		[ClientRpc]
		public static void Play(string sound )
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
