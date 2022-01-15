using Sandbox;
using Sandbox.UI;
using System;

namespace PaintBall
{
	/// <summary>
	/// A <strong><see cref="Sandbox.UI.Panel"></see></strong> that changes width
	/// based on the result of a function. The function should return a value
	/// between 0 and 1.
	/// </summary>
	public class ProgressBar : Panel
	{
		public bool DeleteOnComplete { get; init; }
		/// <summary>
		/// Function that returns a float between 0 and 1.
		/// </summary>
		public Func<float> Fraction { get; init; }
		private RealTimeSince _sinceGetPercentage = 0.25f;
		private Panel _innerPanel;

		public ProgressBar( Func<float> fraction, bool deleteOnComplete = false )
		{
			DeleteOnComplete = deleteOnComplete;
			Fraction = fraction;
			_innerPanel = Add.Panel( "inner" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

			if ( _sinceGetPercentage < 0.2f )
				return;

			_sinceGetPercentage = 0;

			float result = Fraction();

			_innerPanel.Style.Width = Length.Fraction( result );

			if ( result >= 1f && DeleteOnComplete )
				Delete();
		}
	}
}
