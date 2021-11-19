using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class TeamIndicator : Panel
	{
		public Label TeamName;

		public TeamIndicator()
		{
			TeamName = Add.Label( "None" );
		}
	}
}
