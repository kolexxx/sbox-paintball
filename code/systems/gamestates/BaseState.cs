using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace PaintBall
{
	public partial class BaseState : BaseNetworkable
	{
		[Net, Predicted] public TimeSince FreezeTime { get; protected set; } = 5f;
		public virtual bool CanPlayerSuicide => false;
		public virtual int StateDuration => 0;
		public virtual string Name => GetType().Name;
		public float StateEndTime { get; set; }
		public virtual bool UpdateTimer => false;
		protected RealTimeUntil NextSecondTime { get; set; }

		protected static List<Player> Players = new();

		public float TimeLeft
		{
			get
			{
				return StateEndTime - Time.Now;
			}
		}

		[Net] public int TimeLeftSeconds { get; set; }

		public BaseState() { }

		public virtual void AddPlayer( Player player )
		{
			Players.Add( player );
		}

		public virtual void OnPlayerJoin( Player player )
		{
			AddPlayer( player );
		}

		public virtual void OnPlayerLeave( Player player )
		{
			Players.Remove( player );
		}

		public virtual void OnPlayerSpawned( Player player ) { }

		public virtual void OnPlayerKilled( Player player, Entity attacker, DamageInfo info ) { }

		public virtual void OnSecond()
		{
			if ( Host.IsServer )
				TimeLeftSeconds = TimeLeft.CeilToInt();
		}

		public virtual void Tick()
		{
			if ( NextSecondTime <= 0 )
			{
				OnSecond();
				NextSecondTime = 1f;
			}
		}

		public virtual void Start()
		{
			if ( Host.IsServer && StateDuration > 0 )
			{
				Game.Instance.CleanUp();
				StateEndTime = Time.Now + StateDuration;
			}
		}

		public virtual void Finish()
		{
			if ( Host.IsServer )
				return;

			Hud.Reset();
		}
	}
}
