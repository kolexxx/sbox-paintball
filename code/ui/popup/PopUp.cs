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
	public abstract partial class PopUp : Panel
	{
		public Func<bool> Condition { get; set; }
		public RealTimeUntil UntilDelete { get; set; }
		private RealTimeSince _sinceConditionCheck;

		public PopUp( float lifeTime )
		{
			UntilDelete = lifeTime;

			Condition = () => UntilDelete;
		}

		public PopUp( Func<bool> condition )
		{
			Condition = condition;
		}

		public override void Tick()
		{
			if ( _sinceConditionCheck < 0.1f )
				return;

			_sinceConditionCheck = 0f;

			if ( Condition() )
				Delete();
		}
	}
}
