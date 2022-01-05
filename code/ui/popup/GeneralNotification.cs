using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace PaintBall
{
	public class GeneralNotification : Panel
	{
		public Label Message;
		private Panel _currentPopUp;

		public GeneralNotification() { }

		public override void Tick()
		{
			base.Tick();

			if ( _currentPopUp == null )
				return;
		}

		public void AddMessage( string text )
		{
			if ( string.IsNullOrWhiteSpace( text ) )
				return;

			_currentPopUp = Add.Panel();
			Message = _currentPopUp.Add.Label( text );
		}
	}
}
