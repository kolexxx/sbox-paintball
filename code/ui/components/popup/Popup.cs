using Sandbox;
using Sandbox.UI;
using System;

namespace PaintBall
{
	/// <summary>
	/// A <strong><see cref="Sandbox.UI.Panel"></see></strong> that will be deleted 
	/// after some time it was created 
	/// or when a certain condition is met.
	/// </summary>
	public partial class Popup : Panel
	{
		public Func<bool> Condition { get; init; }
		public RealTimeUntil UntilDelete { get; init; }
		public bool HasLifetime { get; init; } = false;
		private RealTimeSince _sinceConditionCheck = 0.2f;

		public Popup( float lifeTime )
		{
			UntilDelete = lifeTime;
			HasLifetime = true;

			Condition = () => UntilDelete;
		}

		public Popup( Func<bool> condition )
		{
			Condition = condition;
		}

		public override void Tick()
		{
			if ( _sinceConditionCheck < 0.2f )
				return;

			_sinceConditionCheck = 0f;

			if ( Condition() )
				Delete();
		}
	}
}
