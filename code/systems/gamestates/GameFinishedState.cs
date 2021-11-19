using Sandbox;

namespace PaintBall
{
	public partial class GameFinishedState : BaseState
	{

		// Broken :)
		public override int StateDuration => 5;

		public override void OnSecond()
		{
			base.OnSecond();

			if ( Time.Now > StateEndTime )
				Finish();
		}

		public override void Start()
		{
			base.Start();

			if ( Host.IsClient )
				Local.Hud.Delete();
		}

		public override void Finish()
		{
			Game.Instance.Delete();
		}
	}
}
