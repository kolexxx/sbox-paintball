using Sandbox;
using Sandbox.UI;
using System;

namespace PaintBall
{
	public class ProgressBar : Panel
	{
		public bool DeleteOnComplete { get; init; }
		public Func<float> Percentage { get; init; }

		public ProgressBar( Func<float> percentage, bool deleteOnComplete )
		{
			DeleteOnComplete = deleteOnComplete;
			Percentage = percentage;
		}

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

			float result = Percentage();

			Style.Width = Length.Percent( result );

			if ( result >= 1f && DeleteOnComplete )
				Delete();
		}
	}
}
