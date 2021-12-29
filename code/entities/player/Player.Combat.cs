using Sandbox;
using System;
using System.Linq;

namespace PaintBall
{
	public enum HitboxIndex
	{
		Pelvis = 1,
		Stomach = 2,
		Rips = 3,
		Neck = 4,
		Head = 5,
		LeftUpperArm = 7,
		LeftLowerArm = 8,
		LeftHand = 9,
		RightUpperArm = 11,
		RightLowerArm = 12,
		RightHand = 13,
		RightUpperLeg = 14,
		RightLowerLeg = 15,
		RightFoot = 16,
		LeftUpperLeg = 17,
		LeftLowerLeg = 18,
		LeftFoot = 19,
	}

	public enum HitboxGroup
	{
		None = -1,
		Generic = 0,
		Head = 1,
		Chest = 2,
		Stomach = 3,
		LeftArm = 4,
		RightArm = 5,
		LeftLeg = 6,
		RightLeg = 7,
		Gear = 10,
		Special = 11,
	}

	public partial class Player
	{
		public int ConsecutiveKills { get; private set; }
		public int KillStreak { get; private set; }
		public DamageInfo LastDamageInfo { get; private set; }
		public TimeSince TimeSinceLastKill { get; private set; }
		private static readonly string[] _consecutiveKillSounds = { "double_kill", "multi_kill", "ultra_kill", "monster_kill" };

		public override void OnKilled()
		{
			base.OnKilled();

			SwitchToBestWeapon();
			Inventory.DropActive();
			Inventory.DeleteContents();

			BecomeRagdollOnClient( LastDamageInfo.Force, GetHitboxBone( LastDamageInfo.HitboxIndex ) );

			RemoveAllDecals();

			var attacker = LastAttacker;

			if ( attacker.IsValid() )
			{
				if ( attacker is Player killer )
					killer?.OnPlayerKill( LastDamageInfo );

				Game.Instance.CurrentGameState?.OnPlayerKilled( this, attacker, LastDamageInfo );
			}
			else
			{
				Game.Instance.CurrentGameState?.OnPlayerKilled( this, null, LastDamageInfo );
			}

			Event.Run( PBEvent.Player.Killed, this, LastAttacker );
			RPC.OnPlayerKilled( this, LastAttacker );
		}

		public override void TakeDamage( DamageInfo info )
		{
			LastDamageInfo = info;

			base.TakeDamage( info );
		}

		protected void OnPlayerKill( DamageInfo info )
		{
			if ( TimeSinceLastKill >= 5f )
				ConsecutiveKills = 0;

			ConsecutiveKills++;

			if ( info.Weapon is Knife )
			{
				Health = 100;

				Audio.Announce( "humiliation", Audio.Priority.Low );
			}
			else if ( ConsecutiveKills >= 2 )
			{
				int index = ConsecutiveKills - 2;
				index = Math.Min( index, 3 );

				Audio.Announce( _consecutiveKillSounds[index], Audio.Priority.Low );
			}
			else if ( info.HitboxIndex == (int)HitboxIndex.Head )
			{
				Audio.Announce( "headshot", Audio.Priority.Low );
			}


			Audio.Play( To.Single( Client ), "kill_confirmed" );

			KillStreak++;
			TimeSinceLastKill = 0f;
		}

		public void SwitchToBestWeapon()
		{
			var best = Children.Select( x => x as Weapon )
			.Where( x => x.IsValid() )
			.OrderBy( x => x.Bucket )
			.FirstOrDefault();

			if ( best == null ) return;

			ActiveChild = best;
		}
	}
}
